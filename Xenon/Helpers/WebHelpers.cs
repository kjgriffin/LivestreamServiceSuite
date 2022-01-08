using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Helpers
{
    public class WebHelpers
    {
        public static readonly HttpClient httpClient = new HttpClient();
    }
}
