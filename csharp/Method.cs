namespace myPOS
{
	public enum Method
	{
		none = 0,
		PURCHASE = 1,
		COMPLETE_TX = 2,
		CANCEL_TX = 3,
		COMPLETE_PREAUTH = 4,
		CANCEL_PREAUTH = 5,
		REVERSAL = 6,
		REFUND = 7,
		GET_STATUS = 8,
		PRINT_EXT = 9,
		ACTIVATE = 10,
		UPDATE = 11,
		REPRINT_RECEIPT = 12,
		DEACTIVATE = 13,
		GET_CERTIFICATE = 14,
		PING = 0xF,
		REBOOT = 0x10,
		GIFTCARD_ACTIVATION = 17,
		GIFTCARD_DEACTIVATION = 18,
		GIFTCARD_CHECK_BALANCE = 19,
		PAYMENT_REQUEST = 20,
		SEND_LOG = 21,
		VENDING_PURCHASE = 22,
		BEEP = 23,
		OPEN_SETTINGS = 24,
		CHECK_TRANSACTION = 25,
		PROCESS = 26,
		CASH_ADVANCE = 27,
		CHECK_CARD = 10001,
		ORIGINAL_CREDIT = 10002
	}
}
