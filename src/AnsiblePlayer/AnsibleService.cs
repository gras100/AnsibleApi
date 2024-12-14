using Microsoft.Extensions.Logging;
using plenidev.AnsiblePlayer.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AnsiblePlayer
{
    public class AnsibleService
    {
        private readonly ILogger _logger;
        private readonly HttpServiceCore _httpServiceCore;
        public AnsibleService(ILogger logger, string baseAddress, string version, string bearerToken)
        {
            _httpServiceCore = HttpServiceCore.Create(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("", "");
                client.DefaultRequestHeaders.Add("", "");
            });
            _logger = logger;
        }
    }
}
