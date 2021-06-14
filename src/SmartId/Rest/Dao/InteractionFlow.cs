namespace SK.SmartId.Rest.Dao
{
    public class InteractionFlow
    {
        public static InteractionFlow DISPLAY_TEXT_AND_PIN = new InteractionFlow("displayTextAndPIN");
        public static InteractionFlow CONFIRMATION_MESSAGE = new InteractionFlow("confirmationMessage");
        public static InteractionFlow VERIFICATION_CODE_CHOICE = new InteractionFlow("verificationCodeChoice");
        public static InteractionFlow CONFIRMATION_MESSAGE_AND_VERIFICATION_CODE_CHOICE = new InteractionFlow("confirmationMessageAndVerificationCodeChoice");

        private InteractionFlow(string code)
        {
            Code = code;
        }

        public string Code { get; }
    }
}