namespace SK.SmartId.Rest.Dao
{
    public class InteractionFlow
    {
        public static readonly InteractionFlow DISPLAY_TEXT_AND_PIN = new InteractionFlow("displayTextAndPIN");
        public static readonly InteractionFlow CONFIRMATION_MESSAGE = new InteractionFlow("confirmationMessage");
        public static readonly InteractionFlow VERIFICATION_CODE_CHOICE = new InteractionFlow("verificationCodeChoice");
        public static readonly InteractionFlow CONFIRMATION_MESSAGE_AND_VERIFICATION_CODE_CHOICE = new InteractionFlow("confirmationMessageAndVerificationCodeChoice");

        private InteractionFlow(string code)
        {
            Code = code;
        }

        public string Code { get; }
    }
}