﻿using System.Net.Http;

namespace Dorbit.Framework.Utils.Http;

public delegate void HttpClientOnException(HttpRequestMessage request, HttpResponseMessage response);