using BlacksmithCore.Infra.Attributes.BlacksmithEnum;
using BlacksmithCore.Infra.Enum;

namespace BlacksmithCore.Infra.Judgement.Core
{
    public class JudgeStage : BlacksmithEnum<JudgeStage>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue OnBegin() => GetCEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue OnEffectSwaping() => GetCEValue();
        [IsBlacksmithEnumMember(16)]
        public CEValue OnApplyingEffect() => GetCEValue();
        [IsBlacksmithEnumMember(32)]
        public CEValue OnEffectTaking_AfterResolutionWritten() => GetCEValue();
        [IsBlacksmithEnumMember(64)]
        public CEValue OnAttackCanceling() => GetCEValue();
        [IsBlacksmithEnumMember(128)]
        public CEValue OnAttackSwaping() => GetCEValue();

        [IsBlacksmithEnumMember(256)]
        public CEValue OnEffectTaking_AfterTransport() => GetCEValue();
        [IsBlacksmithEnumMember(512)]
        public CEValue OnApplyingOthers() => GetCEValue();
        [IsBlacksmithEnumMember(1024)]
        public CEValue OnUpdating() => GetCEValue();
        [IsBlacksmithEnumMember(2048)]
        public CEValue OnEffectTaking_AfterResult() => GetCEValue();
        [IsBlacksmithEnumMember(4096)]
        public CEValue OnEnd() => GetCEValue();
    }
}
