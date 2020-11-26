using _2CaptchaAPI;
using Bolly.Interfaces;
using Bolly.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bolly.Blocks
{
    public class BlockCaptchaSolver : BlockBase
    {
        protected class CaptchaSoler
        {
            public string ApiKey { get; set; }
            public string Type { get; set; }
            public string Url { get; set; }
            public string SiteKey { get; set; }
            public bool Invisible { get; set; }
        }

        protected class ReCaptchaV2 : ICaptchaSolver
        {
            private readonly _2Captcha _2Captcha;
            private readonly string _siteKey;
            private readonly string _url;
            private readonly bool _invisible;

            public ReCaptchaV2(_2Captcha _2Captcha, string siteKey, string url, bool invisible)
            {
                this._2Captcha = _2Captcha;
                _siteKey = siteKey;
                _url = url;
                _invisible = invisible;
            }

            public async Task<_2Captcha.Result> Execute()
            {
                return await _2Captcha.SolveReCaptchaV2(_siteKey, _url, _invisible);
            }
        }

        protected class ReCaptchaV3 : ICaptchaSolver
        {
            private readonly _2Captcha _2Captcha;
            private readonly string _siteKey;
            private readonly string _url;

            public ReCaptchaV3(_2Captcha _2Captcha, string siteKey, string url)
            {
                this._2Captcha = _2Captcha;
                _siteKey = siteKey;
                _url = url;
            }

            public async Task<_2Captcha.Result> Execute()
            {
                return await _2Captcha.SolveReCaptchaV3(_siteKey, _url);
            }
        }

        protected class HCaptcha : ICaptchaSolver
        {
            private readonly _2Captcha _2Captcha;
            private readonly string _siteKey;
            private readonly string _url;

            public HCaptcha(_2Captcha _2Captcha, string siteKey, string url)
            {
                this._2Captcha = _2Captcha;
                _siteKey = siteKey;
                _url = url;
            }

            public async Task<_2Captcha.Result> Execute()
            {
                return await _2Captcha.SolveHCaptcha(_siteKey, _url);
            }
        }

        private readonly CaptchaSoler _captchaSolver;
        private readonly ICaptchaSolver _captchaSolverProcess;

        private const string _captchaSolutionVariableName = "SOLUTION";

        public BlockCaptchaSolver(string jsonString)
        {
            _captchaSolver = JsonSerializer.Deserialize<CaptchaSoler>(jsonString);

            var captcha = new _2Captcha(_captchaSolver.ApiKey);

            switch (_captchaSolver.Type.ToLower())
            {
                case "recaptchav2":
                    _captchaSolverProcess = new ReCaptchaV2(captcha, _captchaSolver.SiteKey, _captchaSolver.Url, _captchaSolver.Invisible);
                    break;
                case "recaptchav3":
                    _captchaSolverProcess = new ReCaptchaV3(captcha, _captchaSolver.SiteKey, _captchaSolver.Url);
                    break;
                case "hcaptcha":
                    _captchaSolverProcess = new HCaptcha(captcha, _captchaSolver.SiteKey, _captchaSolver.Url);
                    break;
            }
        }

        public override async Task Execute(HttpClient httpclient, BotData botData)
        {
            _2Captcha.Result result;

            while (true)
            {
                result = await _captchaSolverProcess.Execute();
                if (result.Success) break;
            }

            if (!botData.Variables.TryAdd(_captchaSolutionVariableName, result.Response)) botData.Variables[_captchaSolutionVariableName] = result.Response;
        }
    }
}
