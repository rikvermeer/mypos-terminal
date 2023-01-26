using System;

namespace myPOS
{
	public class TransactionData
	{
		public string Type;

		public string AuthCode;

		public string Approval;

		public DateTime TransactionDate;

		public string RRN;

		public string Amount;

		public string TipAmount;

		public Currencies Currency;

		public string TerminalID;

		public string MerchantID;

		public string MerchantName;

		public string MerchantAddressLine1;

		public string MerchantAddressLine2;

		public string PANMasked;

		public string PreauthCode;

		public string EmbossName;

		public string AID;

		public string AIDName;

		public string ApplicationPreferredName;

		public string Stan;

		public bool SignatureRequired;

		public string SoftwareVersion;
	}
}
