namespace Blacksmith.Backend.JudgementLogic.Core
{
    public enum AttackType
    {
        Physical,
        Magic,
        Real,
    }/*
    public class AttackType : BlacksmithEnum<AttackType>
    {
        [IsBlacksmithEnumMember(256)]
        public EEValue Physical() => GetEEValue();
        [IsBlacksmithEnumMember(128)]
        public EEValue Magical() => GetEEValue();
        [IsBlacksmithEnumMember(0)]
        public EEValue Real() => GetEEValue();
    }*/
}