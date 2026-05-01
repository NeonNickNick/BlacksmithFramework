using BlacksmithCore.Infra.Models;

namespace BlacksmithCore.Backend.JudgementLogic.Judgement.Core
{
    public class Intent
    {
        public Action<Community> Execute { get; set; }
    }
}
