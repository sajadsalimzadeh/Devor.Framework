﻿using Dorbit.Framework.Attributes;
using Dorbit.Framework.Models.Users;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using UAParser;

namespace Dorbit.Framework.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
internal class UserStateService : IUserStateService
{
    private readonly Dictionary<Guid, UserState> _states = new();
    private readonly IGeoService _geoService;
    private readonly AppSetting _appSetting;

    public UserStateService(IGeoService geoService, AppSetting appSetting)
    {
        _geoService = geoService;
        _appSetting = appSetting;
    }

    public UserState GetUserState(Guid userId)
    {
        lock (_states)
        {
            if (!_states.TryGetValue(userId, out var result))
            {
                result = new UserState(userId);
                _states[userId] = result;
            }
            else if (result.LastRequestTime < DateTime.UtcNow.AddMinutes(-5))
            {
                _states.Remove(userId);
                return GetUserState(userId);
            }

            return result;
        }
    }

    public IEnumerable<UserState> GetOnlineUsers()
    {
        var time = DateTime.UtcNow.AddMinutes(-1);
        lock (_states)
        {
            return _states.Values.Where(x => x?.LastRequestTime > time);
        }
    }

    public void LoadClientInfo(UserState state, string uaString)
    {
        try
        {
            var uaParser = Parser.GetDefault();
            state.ClientInfo = uaParser.Parse(uaString);
        }
        catch
        {
            // ignored
        }
    }

    public void LoadGeoInfo(UserState state, string ip)
    {
        if (!_appSetting.Geo.Enable) return;
        lock (state)
        {
            if (state.IsGeoInfoInquiry) return;
            state.IsGeoInfoInquiry = true;
        }

        new Thread(Start).Start();
        return;

        async void Start()
        {
            try
            {
                state.GeoInfo = (await _geoService.GetGeoInfoAsync(ip)).Result;
            }
            catch
            {
                // ignored
            }
        }
    }
}