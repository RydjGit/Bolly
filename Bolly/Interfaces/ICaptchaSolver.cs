using _2CaptchaAPI;
using System.Threading.Tasks;

namespace Bolly.Interfaces
{
    public interface ICaptchaSolver
    {
        public Task<_2Captcha.Result> Execute();
    }
}
