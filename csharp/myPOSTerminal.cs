using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace myPOS
{
	public class myPOSTerminal
	{
		private class Certificate
		{
			public byte[] Fingerprint;

			public byte[] Data;
		}

		public delegate void ProcessingResultHandler(ProcessingResult result);

		public delegate void LogHandler(string message);

		public delegate void onPresentCardHandler();

		public delegate void onCardDetectedHandler(bool is_bad_card);

		public delegate void onPresentPinHandler(int tries_left);

		public delegate void onPinEnteredHandler(bool is_wrong_pin, int tries_left);

		public delegate void onPresentDCCHandler(DCCRequest dcc_request);

		public delegate void onDCCSelectedHandler(bool is_dcc_used);

		public delegate string onApprovalGetReceiptDataHandler();

		private string ComPortName;

		private bool Initialized;

		private bool Abort;

		private string Lang = "";

		private string Password = "";

		private ReceiptMode receipt_mode = ReceiptMode.NotConfugred;

		private bool Printer;

		private bool PresentPinInvoked;

		private string LastTransactionSID;

		private int TimeoutTime = 3;

		private int COMTimeoutTime = 3000;

		private Method LastUsedMethod;

		private string LastReceivedStage;

		private Thread CommunicationThread;

		private SerialPort comPort;

		private byte[] Chain;

		private string HostIP;

		private ushort HostPort;

		private List<Certificate> Certificates = new List<Certificate>();

		private TcpClient client = new TcpClient();

		private SslStream sslStream;

		private Stream commStream;

		public bool isFixedPinpad { get; set; }

		public bool HasPrinter => Printer;

		public event ProcessingResultHandler ProcessingFinished;

		public event LogHandler Log;

		public event onPresentCardHandler onPresentCard;

		public event onCardDetectedHandler onCardDetected;

		public event onPresentPinHandler onPresentPin;

		public event onPinEnteredHandler onPinEntered;

		public event onPresentDCCHandler onPresentDCC;

		public event onDCCSelectedHandler onDCCSelected;

		public event onApprovalGetReceiptDataHandler onApprovalGetReceiptData;

		private RequestResult StartCommunicationWithTerminal(IPPProcessor request)
		{
			Abort = false;
			if (CommunicationThread != null && CommunicationThread.IsAlive)
			{
				return RequestResult.Busy;
			}
			CommunicationThread = new Thread(new ParameterizedThreadStart(CommunicateWithTerminal));
			CommunicationThread.Start(request);
			return RequestResult.Processing;
		}

		private void AddLog(string m)
		{
			Console.WriteLine(m);
			if (this.Log != null)
			{
				this.Log(m);
			}
		}

		private void CommunicateWithTerminal(object param)
		{
			//IL_0442: Unknown result type (might be due to invalid IL or missing references)
			//IL_044c: Expected O, but got Unknown
			IPPProcessor iPPProcessor = (IPPProcessor)param;
			string text = null;
			string text2 = null;
			string text3 = null;
			string text4 = null;
			ProcessingResult processingResult = new ProcessingResult();
			IPPProcessor.Field field = iPPProcessor.Get("METHOD");
			if (field != null)
			{
				text = field.str_data;
			}
			Enum.TryParse<Method>(text, out processingResult.Method);
			ParseMethod(text, out LastUsedMethod);
			try
			{
				comPort.DiscardInBuffer();
				comPort.DiscardOutBuffer();
				while (true)
				{
					SendRequestToTerminal(iPPProcessor);
					if (text == "REPRINT_RECEIPT")
					{
						TimeoutTime = 50;
					}
					else
					{
						TimeoutTime = 3;
					}
					while (true)
					{
						ResultCommunication resultCommunication = ReceiveMessage(out var result);
						if (resultCommunication == ResultCommunication.ExpectAnotherMessage)
						{
							continue;
						}
						IPPProcessor iPPProcessor2;
						if (resultCommunication != 0)
						{
							if (resultCommunication == ResultCommunication.Timeout)
							{
								processingResult.Status = TransactionStatus.Timeout;
							}
							else
							{
								processingResult.Status = TransactionStatus.InternalError;
							}
						}
						else
						{
							field = result.Get("STATUS");
							if (field != null)
							{
								text2 = field.str_data;
							}
							field = result.Get("STAGE");
							if (field != null)
							{
								text3 = field.str_data;
							}
							field = result.Get("METHOD");
							if (field != null)
							{
								text4 = field.str_data;
							}
							if (text == "GET_STATUS" && text4 == "GET_CERTIFICATE" && text2 == "0")
							{
								Thread.Sleep(100);
								break;
							}
							if (text2 == "20" && text != "GET_STATUS" && text != "VENDING_PURCHASE")
							{
								iPPProcessor2 = IPPProcessor.CreateRequest("COMPLETE_TX");
								iPPProcessor2.Add("SID_ORIGINAL", LastTransactionSID);
								SendRequestToTerminal(iPPProcessor2);
								continue;
							}
							if (text4 == "COMPLETE_TX" && text != "COMPLETE_TX" && text2 == "0")
							{
								break;
							}
							ParseMethod(text, out processingResult.Method);
							if (int.TryParse(text2, out var result2) && Enum.IsDefined(typeof(TransactionStatus), result2))
							{
								processingResult.Status = (TransactionStatus)result2;
							}
							if (text3 == "5")
							{
								switch (processingResult.Method)
								{
								case Method.PURCHASE:
								case Method.COMPLETE_PREAUTH:
								case Method.CANCEL_PREAUTH:
								case Method.REVERSAL:
								case Method.REFUND:
								case Method.GET_STATUS:
								case Method.VENDING_PURCHASE:
								case Method.CASH_ADVANCE:
									ParseTransactionData(result, out processingResult.TranData);
									break;
								}
							}
						}
						if (this.ProcessingFinished != null)
						{
							this.ProcessingFinished(processingResult);
						}
						if (text3 == "5")
						{
							switch (text2)
							{
							case "0":
							case "100":
							case "22":
								break;
							default:
								goto IL_02fe;
							}
							switch (text)
							{
							case "PURCHASE":
							case "REFUND":
							case "REVERSAL":
							case "GIFTCARD_ACTIVATION":
							case "GIFTCARD_DEACTIVATION":
							case "GIFTCARD_CHECK_BALANCE":
							case "ORIGINAL_CREDIT":
							case "CASH_ADVANCE":
								goto IL_0331;
							}
						}
						goto IL_02fe;
						IL_02fe:
						if (!(text3 == "5") || !(text == "GET_STATUS") || !(text2 == "20"))
						{
							return;
						}
						goto IL_0331;
						IL_0331:
						Thread.Sleep(2000);
						string paramData = "";
						if (this.onApprovalGetReceiptData != null && text != "REVERSAL" && text != "GIFTCARD_ACTIVATION" && text != "GIFTCARD_DEACTIVATION" && text != "GIFTCARD_CHECK_BALANCE")
						{
							paramData = this.onApprovalGetReceiptData();
						}
						iPPProcessor2 = IPPProcessor.CreateRequest("COMPLETE_TX");
						iPPProcessor2.Add("SID_ORIGINAL", LastTransactionSID);
						iPPProcessor2.Add("CREDENTIAL", paramData);
						SendRequestToTerminal(iPPProcessor2);
						do
						{
							resultCommunication = ReceiveMessage(out result);
						}
						while (resultCommunication == ResultCommunication.ExpectAnotherMessage);
						return;
					}
				}
			}
			catch (Exception ex)
			{
				AddLog("[Exception] " + ex.Message);
				processingResult.Status = TransactionStatus.InternalError;
				if (this.ProcessingFinished != null)
				{
					this.ProcessingFinished(processingResult);
				}
				try
				{
					AddLog("Trying to reconnect to " + ComPortName);
					comPort.Close();
					Thread.Sleep(50);
					comPort = new SerialPort(ComPortName, 115200, (Parity)0, 8, (StopBits)1);
					//comPort.set_ReadTimeout(COMTimeoutTime);
					//comPort.set_WriteTimeout(COMTimeoutTime);
                    comPort.ReadTimeout = COMTimeoutTime;
					comPort.WriteTimeout = COMTimeoutTime;
					comPort.Open();
					comPort.DiscardInBuffer();
					comPort.DiscardOutBuffer();
				}
				catch (Exception ex2)
				{
					AddLog("[Exception] " + ex2.Message);
					comPort = null;
					Initialized = false;
				}
			}
		}

		private int ReceiveFromComPort(byte[] buffer, int offset, int len, int timeout)
		{
			int num = 0;
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = new TimeSpan(0, 0, timeout);
			do
			{
				//if (comPort.get_BytesToRead() > 0)
                if (comPort.BytesToRead > 0)
				{
					num += comPort.Read(buffer, offset + num, len - num);
				}
			}
			while (!(DateTime.Now - now > timeSpan) && num < len && !Abort);
			return num;
		}

		private void SendRequestToTerminal(IPPProcessor request)
		{
			string text = "Sending to terminal:";
			foreach (IPPProcessor.Field field in request.fields)
			{
				text = text + "\r\n" + field.name + "=" + field.str_data;
				if (field.bin_data != null)
				{
					text += "0x";
					if (field.name == "DATA" || field.name == "CERT")
					{
						text += BitConverter.ToString(new byte[2]
						{
							(byte)(field.bin_data_size / 256),
							(byte)(field.bin_data_size % 256)
						}).Replace("-", "");
					}
					text += BitConverter.ToString(field.bin_data).Replace("-", "");
				}
			}
			AddLog(text);
			SendToComPort(request.GetDataForSending());
		}

		private void SendToComPort(byte[] data)
		{
			Thread.Sleep(100);
			comPort.Write(data, 0, data.Length);
		}

		private ResultCommunication ReceiveMessage(out IPPProcessor result)
		{
			result = null;
			string text = null;
			string text2 = null;
			string text3 = null;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			byte[] array = new byte[2];
			AddLog("Waiting for input");
			num2 = ReceiveFromComPort(array, 0, 2, TimeoutTime);
			if (Abort)
			{
				return ResultCommunication.Aborted;
			}
			if (num2 < 2)
			{
				return ResultCommunication.Timeout;
			}
			num3 = (array[0] << 8) + array[1] - 2;
			if (Abort)
			{
				return ResultCommunication.Aborted;
			}
			if (num3 <= 0)
			{
				AddLog("Expected message lenght is <=0.");
				return ResultCommunication.Error;
			}
			byte[] array2 = new byte[num3];
			num2 = ReceiveFromComPort(array2, 0, num3, 5);
			AddLog("Input received");
			string text4 = "RAW:";
			text4 += BitConverter.ToString(array).Replace("-", "");
			text4 += BitConverter.ToString(array2).Replace("-", "");
			AddLog(text4);
			if (num2 != num3)
			{
				AddLog($"Expected lenght: {num3}, received: {num2}. ");
				return ResultCommunication.Error;
			}
			if (!IPPProcessor.TryParse(array2, out result))
			{
				return ResultCommunication.Error;
			}
			string text5 = "Message:";
			foreach (IPPProcessor.Field field2 in result.fields)
			{
				text5 = text5 + "\r\n" + field2.name + "=" + field2.str_data;
				if (field2.bin_data != null)
				{
					text5 = text5 + "0x" + BitConverter.ToString(field2.bin_data).Replace("-", "");
				}
			}
			AddLog(text5);
			if (!result.IsValid())
			{
				return ResultCommunication.Error;
			}
			ProcessDeviceParameters(result);
			IPPProcessor.Field field = result.Get("TIMEOUT");
			if (field != null)
			{
				int.TryParse(field.str_data, out TimeoutTime);
			}
			if (TimeoutTime <= 0)
			{
				TimeoutTime = 5;
			}
			field = result.Get("METHOD");
			if (field != null)
			{
				text = field.str_data;
			}
			field = result.Get("STAGE");
			if (field != null)
			{
				text2 = field.str_data;
			}
			LastReceivedStage = text2;
			field = result.Get("STATUS");
			if (field != null)
			{
				text3 = field.str_data;
			}
			if (text == null)
			{
				return ResultCommunication.Error;
			}
			switch (text)
			{
			case "GET_STATUS":
				if (text2 == "5")
				{
					Initialized = true;
					field = result.Get("SID_ORIGINAL");
					if (field != null)
					{
						LastTransactionSID = field.str_data;
					}
					byte[] array4 = NeedCertificate();
					if (array4 != null)
					{
						Thread.Sleep(100);
						IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("GET_CERTIFICATE");
						iPPProcessor.Add("FINGERPRINT", array4);
						SendRequestToTerminal(iPPProcessor);
						return ResultCommunication.ExpectAnotherMessage;
					}
					return ResultCommunication.Finished;
				}
				return ResultCommunication.ExpectAnotherMessage;
			case "GET_CERTIFICATE":
			{
				if (text3 != "0")
				{
					return ResultCommunication.Finished;
				}
				if (text2 != "5")
				{
					return ResultCommunication.ExpectAnotherMessage;
				}
				field = result.Get("FINGERPRINT");
				if (field == null)
				{
					return ResultCommunication.Error;
				}
				byte[] bin_data = field.bin_data;
				field = result.Get("CERT");
				AddCertificate(bin_data, field.bin_data.Skip(2).ToArray());
				byte[] array4 = NeedCertificate();
				if (array4 != null)
				{
					Thread.Sleep(100);
					IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("GET_CERTIFICATE");
					iPPProcessor.Add("FINGERPRINT", array4);
					SendRequestToTerminal(iPPProcessor);
					return ResultCommunication.ExpectAnotherMessage;
				}
				return ResultCommunication.Finished;
			}
			case "REPRINT_RECEIPT":
				return ResultCommunication.Finished;
			default:
				switch (text2)
				{
				case null:
					return ResultCommunication.ExpectAnotherMessage;
				case "1":
					PresentPinInvoked = false;
					if (text == "PRINT_EXT")
					{
						return ResultCommunication.Finished;
					}
					if (text3 == null)
					{
						return ResultCommunication.Error;
					}
					if (text3 != "0")
					{
						return ResultCommunication.Finished;
					}
					return ResultCommunication.ExpectAnotherMessage;
				case "2":
					switch (text3)
					{
					case "52":
					case "53":
					case "54":
						return ResultCommunication.ExpectAnotherMessage;
					case "0":
					case "6":
						if (this.onCardDetected != null)
						{
							this.onCardDetected((!(text3 == "0")) ? true : false);
						}
						return ResultCommunication.ExpectAnotherMessage;
					default:
						return ResultCommunication.Finished;
					}
				case "3":
					if (text3 == "0" || text3 == "9")
					{
						if (PresentPinInvoked)
						{
							num = 0;
							field = result.Get("TRIES_LEFT");
							if (field != null)
							{
								int.TryParse(field.str_data, out num);
							}
							if (this.onPinEntered != null)
							{
								this.onPinEntered((!(text3 == "0")) ? true : false, num);
							}
						}
						return ResultCommunication.ExpectAnotherMessage;
					}
					return ResultCommunication.Finished;
				case "4":
					if (text3 == null)
					{
						return ResultCommunication.Error;
					}
					switch (text3)
					{
					case "0":
					{
						field = result.Get("DATA");
						if (field == null)
						{
							return ResultCommunication.Error;
						}
						byte[] array3 = HostSendRcv(field.bin_data);
						HostClose();
						if (Abort)
						{
							return ResultCommunication.Aborted;
						}
						if (array3 != null)
						{
							IPPProcessor iPPProcessor = result.CreateResult(0);
							iPPProcessor.Add("DATA", array3);
							SendRequestToTerminal(iPPProcessor);
						}
						break;
					}
					case "32":
					{
						field = result.Get("DATA");
						if (field == null)
						{
							return ResultCommunication.Error;
						}
						byte[] array3 = HostSendRcv(field.bin_data);
						if (Abort)
						{
							return ResultCommunication.Aborted;
						}
						if (array3 != null)
						{
							IPPProcessor iPPProcessor = result.CreateResult(0);
							iPPProcessor.Add("DATA", array3);
							SendRequestToTerminal(iPPProcessor);
						}
						break;
					}
					case "31":
						field = result.Get("DATA");
						if (field == null)
						{
							return ResultCommunication.Error;
						}
						HostSend(field.bin_data);
						break;
					default:
						return ResultCommunication.Finished;
					}
					return ResultCommunication.ExpectAnotherMessage;
				case "5":
					switch (text)
					{
					case "PURCHASE":
					case "REFUND":
					case "GIFTCARD_ACTIVATION":
					case "GIFTCARD_DEACTIVATION":
					case "GIFTCARD_CHECK_BALANCE":
					case "CHECK_CARD":
					case "ORIGINAL_CREDIT":
					case "VENDING_PURCHASE":
					case "CASH_ADVANCE":
						field = result.Get("SID");
						if (field != null)
						{
							LastTransactionSID = field.str_data;
						}
						break;
					case "CHECK_TRANSACTION":
						if (text3 == "0")
						{
							HostClose();
							return ResultCommunication.ExpectAnotherMessage;
						}
						break;
					}
					HostClose();
					return ResultCommunication.Finished;
				case "11":
					if (this.onPresentCard != null)
					{
						this.onPresentCard();
					}
					return ResultCommunication.ExpectAnotherMessage;
				case "13":
					PresentPinInvoked = true;
					num = 0;
					field = result.Get("TRIES_LEFT");
					if (field != null)
					{
						int.TryParse(field.str_data, out num);
					}
					if (this.onPresentPin != null)
					{
						this.onPresentPin(num);
					}
					return ResultCommunication.ExpectAnotherMessage;
				case "12":
				{
					DCCRequest dCCRequest = new DCCRequest();
					field = result.Get("AMOUNT");
					if (field != null)
					{
						dCCRequest.OriginalAmount = field.str_data;
					}
					field = result.Get("CURRENCY");
					if (field != null)
					{
						dCCRequest.OriginalCurrencyCode = field.str_data;
					}
					field = result.Get("CURRENCY_NAME");
					if (field != null)
					{
						dCCRequest.OriginalCurrencyName = field.str_data;
					}
					field = result.Get("DCC_AMOUNT");
					if (field != null)
					{
						dCCRequest.DCCAmount = field.str_data;
					}
					field = result.Get("DCC_CURRENCY");
					if (field != null)
					{
						dCCRequest.DCCCurrencyCode = field.str_data;
					}
					field = result.Get("DCC_CURRENCY_NAME");
					if (field != null)
					{
						dCCRequest.DCCCurrencyName = field.str_data;
					}
					field = result.Get("DCC_EXCHANGE_RATE");
					if (field != null)
					{
						dCCRequest.DCCExchangeRate = field.str_data;
					}
					if (this.onPresentDCC != null)
					{
						this.onPresentDCC(dCCRequest);
					}
					return ResultCommunication.ExpectAnotherMessage;
				}
				case "14":
				{
					bool is_dcc_used = false;
					field = result.Get("DCC_USED");
					if (field != null && field.str_data == "1")
					{
						is_dcc_used = true;
					}
					if (this.onDCCSelected != null)
					{
						this.onDCCSelected(is_dcc_used);
					}
					return ResultCommunication.ExpectAnotherMessage;
				}
				default:
					if (text3 == null)
					{
						return ResultCommunication.Error;
					}
					if (text3 != "0")
					{
						return ResultCommunication.Finished;
					}
					return ResultCommunication.ExpectAnotherMessage;
				}
			}
		}

		private void ProcessDeviceParameters(IPPProcessor msg)
		{
			IPPProcessor.Field field = msg.Get("IP");
			if (field != null)
			{
				HostIP = field.str_data;
			}
			field = msg.Get("PORT");
			if (field != null)
			{
				ushort.TryParse(field.str_data, out HostPort);
			}
			field = msg.Get("PRIMARY_CHAIN");
			if (field != null)
			{
				NewFingerprint(field.bin_data);
			}
			field = msg.Get("SECONDARY_CHAIN");
			if (field != null)
			{
				NewFingerprint(field.bin_data);
			}
			field = msg.Get("CHAIN");
			if (field != null)
			{
				Chain = field.bin_data;
			}
			field = msg.Get("PRINTER");
			if (field != null)
			{
				Printer = field.str_data == "1";
			}
		}

		private void NewFingerprint(byte[] new_fingerprint_data)
		{
			int num = 0;
			if (new_fingerprint_data != null)
			{
				num = new_fingerprint_data.Length / 21;
				for (int i = 0; i < num; i++)
				{
					AddCertificate(new_fingerprint_data.Skip(i * 21).Take(20).ToArray(), null);
				}
			}
		}

		private void ParseMethod(string method, out Method data)
		{
			switch (method)
			{
			case "PURCHASE":
				data = Method.PURCHASE;
				break;
			case "COMPLETE_TX":
				data = Method.COMPLETE_TX;
				break;
			case "CANCEL_TX":
				data = Method.CANCEL_TX;
				break;
			case "COMPLETE_PREAUTH":
				data = Method.COMPLETE_PREAUTH;
				break;
			case "CANCEL_PREAUTH":
				data = Method.CANCEL_PREAUTH;
				break;
			case "REVERSAL":
				data = Method.REVERSAL;
				break;
			case "REFUND":
				data = Method.REFUND;
				break;
			case "GET_STATUS":
				data = Method.GET_STATUS;
				break;
			case "PRINT_EXT":
				data = Method.PRINT_EXT;
				break;
			case "ACTIVATE":
				data = Method.ACTIVATE;
				break;
			case "UPDATE":
				data = Method.UPDATE;
				break;
			case "REPRINT_RECEIPT":
				data = Method.REPRINT_RECEIPT;
				break;
			case "DEACTIVATE":
				data = Method.DEACTIVATE;
				break;
			case "GET_CERTIFICATE":
				data = Method.GET_CERTIFICATE;
				break;
			case "PING":
				data = Method.PING;
				break;
			case "REBOOT":
				data = Method.REBOOT;
				break;
			case "GIFTCARD_ACTIVATION":
				data = Method.GIFTCARD_ACTIVATION;
				break;
			case "GIFTCARD_DEACTIVATION":
				data = Method.GIFTCARD_DEACTIVATION;
				break;
			case "GIFTCARD_CHECK_BALANCE":
				data = Method.GIFTCARD_CHECK_BALANCE;
				break;
			case "PAYMENT_REQUEST":
				data = Method.PAYMENT_REQUEST;
				break;
			case "SEND_LOG":
				data = Method.SEND_LOG;
				break;
			case "VENDING_PURCHASE":
				data = Method.VENDING_PURCHASE;
				break;
			case "BEEP":
				data = Method.BEEP;
				break;
			case "OPEN_SETTINGS":
				data = Method.OPEN_SETTINGS;
				break;
			case "CHECK_TRANSACTION":
				data = Method.CHECK_TRANSACTION;
				break;
			case "PROCESS":
				data = Method.PROCESS;
				break;
			case "CASH_ADVANCE":
				data = Method.CASH_ADVANCE;
				break;
			case "CHECK_CARD":
				data = Method.CHECK_CARD;
				break;
			case "ORIGINAL_CREDIT":
				data = Method.ORIGINAL_CREDIT;
				break;
			default:
				data = Method.none;
				break;
			}
		}

		private void ParseTransactionData(IPPProcessor msg, out TransactionData data)
		{
			string text = null;
			string s = null;
			string text2 = null;
			string text3 = null;
			IPPProcessor.Field field = msg.Get("METHOD");
			if (field != null)
			{
				text = field.str_data;
			}
			switch (text)
			{
			case "GET_STATUS":
				field = msg.Get("LAST_TX_TYPE");
				if (field != null)
				{
					switch (field.str_data)
					{
					case "00":
						text = "PURCHASE";
						break;
					case "20":
						text = "REFUND";
						break;
					case "02":
						text = "REVERSAL";
						break;
					case "01":
						text = "CASH_ADVANCE";
						break;
					case "05":
						text = "CHECK_CARD";
						break;
					case "28":
						text = "ORIGINAL_CREDIT";
						break;
					default:
						data = null;
						return;
					}
				}
				break;
			default:
				data = null;
				return;
			case "PURCHASE":
			case "REFUND":
			case "REVERSAL":
			case "COMPLETE_PREAUTH":
			case "CANCEL_PREAUTH":
			case "VENDING_PURCHASE":
			case "CASH_ADVANCE":
			case "CHECK_CARD":
			case "ORIGINAL_CREDIT":
				break;
			}
			data = new TransactionData();
			data.Type = text;
			field = msg.Get("AID");
			if (field != null)
			{
				data.AID = field.str_data;
			}
			field = msg.Get("AID_NAME");
			if (field != null)
			{
				data.AIDName = field.str_data;
			}
			field = msg.Get("APPL_PREF_NAME");
			if (field != null)
			{
				data.ApplicationPreferredName = field.str_data;
			}
			field = msg.Get("AMOUNT");
			if (field != null)
			{
				data.Amount = field.str_data;
			}
			field = msg.Get("TIP_AMOUNT");
			if (field != null)
			{
				data.TipAmount = field.str_data;
			}
			field = msg.Get("APPROVAL");
			if (field != null)
			{
				data.Approval = field.str_data;
			}
			field = msg.Get("AUTH_CODE");
			if (field != null)
			{
				data.AuthCode = field.str_data;
			}
			field = msg.Get("CURRENCY");
			if (field != null)
			{
				s = field.str_data;
			}
			if (int.TryParse(s, out var result) && Enum.IsDefined(typeof(Currencies), result))
			{
				data.Currency = (Currencies)result;
			}
			field = msg.Get("EMBOSS_NAME");
			if (field != null)
			{
				data.EmbossName = field.str_data;
			}
			field = msg.Get("MERCHANT_AL1");
			if (field != null)
			{
				data.MerchantAddressLine1 = field.str_data;
			}
			field = msg.Get("MERCHANT_AL2");
			if (field != null)
			{
				data.MerchantAddressLine2 = field.str_data;
			}
			field = msg.Get("MERCHANT_ID");
			if (field != null)
			{
				data.MerchantID = field.str_data;
			}
			field = msg.Get("MERCHANT_NAME");
			if (field != null)
			{
				data.MerchantName = field.str_data;
			}
			field = msg.Get("PAN_MASKED");
			if (field != null)
			{
				data.PANMasked = field.str_data;
			}
			field = msg.Get("PREAUTH_CODE");
			if (field != null)
			{
				data.PreauthCode = field.str_data;
			}
			field = msg.Get("RRN");
			if (field != null)
			{
				data.RRN = field.str_data;
			}
			field = msg.Get("SIGNATURE_NOT_REQ");
			if (field != null)
			{
				data.SignatureRequired = field.str_data == "0";
			}
			field = msg.Get("STAN");
			if (field != null)
			{
				data.Stan = field.str_data;
			}
			field = msg.Get("TERMINAL_ID");
			if (field != null)
			{
				data.TerminalID = field.str_data;
			}
			field = msg.Get("TX_DATE_LOCAL");
			if (field != null)
			{
				text2 = field.str_data;
			}
			field = msg.Get("TX_TIME_LOCAL");
			if (field != null)
			{
				text3 = field.str_data;
			}
			field = msg.Get("SOFTWARE_VERSION");
			if (field != null)
			{
				data.SoftwareVersion = field.str_data;
			}
			if (text2 != null && text3 != null && DateTime.TryParse(text2 + " " + text3, out var result2))
			{
				data.TransactionDate = result2;
			}
		}

		private void AddCertificate(byte[] Fingerprint, byte[] Data)
		{
			Certificate certificate = Certificates.FirstOrDefault((Certificate x) => x.Fingerprint.SequenceEqual(Fingerprint));
			BitConverter.ToString(Fingerprint).Replace("-", "");
			if (certificate == null)
			{
				certificate = new Certificate();
				certificate.Fingerprint = Fingerprint;
				Certificates.Add(certificate);
			}
			if (Data != null)
			{
				certificate.Data = Data;
			}
		}

		private byte[] NeedCertificate()
		{
			return Certificates.FirstOrDefault((Certificate x) => x.Data == null)?.Fingerprint;
		}

		private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None || sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
			{
				return true;
			}
			return false;
		}

		private bool Connect()
		{
			try
			{
				client = new TcpClient(HostIP, HostPort);
				if (Chain != null && Chain.Length != 0)
				{
					sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
					sslStream.AuthenticateAsClient(HostIP, null, SslProtocols.Tls12, checkCertificateRevocation: false);
					commStream = sslStream;
				}
				else
				{
					commStream = client.GetStream();
				}
				return true;
			}
			catch (Exception ex)
			{
				AddLog("[Exception] " + ex.Message);
				client.Close();
			}
			return false;
		}

		private void HostClose()
		{
			if (commStream != null)
			{
				commStream.Close();
			}
			if (client != null && client.Connected)
			{
				client.Close();
			}
		}

		private byte[] HostSendRcv(byte[] DataToSend)
		{
			if (!client.Connected && !Connect())
			{
				return null;
			}
			if (DataToSend != null)
			{
				commStream.Write(DataToSend, 0, DataToSend.Length);
			}
			byte[] array = new byte[2];
			byte[] array2 = null;
			if (commStream.Read(array, 0, 2) == 2)
			{
				int num = (array[0] << 8) + array[1];
				array2 = new byte[num];
				if (ReadWithTimeout(commStream, array2, 0, num) != num)
				{
					return null;
				}
				return array2;
			}
			return null;
		}

		private void HostSend(byte[] DataToSend)
		{
			if ((client.Connected || Connect()) && DataToSend != null)
			{
				sslStream.Write(DataToSend, 0, DataToSend.Length);
			}
		}

		private int ReadWithTimeout(Stream stream, byte[] buffer, int offset, int size)
		{
			int tickCount = Environment.TickCount;
			int num = 0;
			while (Environment.TickCount <= tickCount + 40000)
			{
				try
				{
					num += stream.Read(buffer, offset + num, size - num);
				}
				catch (Exception ex)
				{
					AddLog("[Exception] " + ex.Message);
					return num;
				}
				if (num >= size)
				{
					break;
				}
			}
			return num;
		}

		public RequestResult Initialize(string ComPort)
		{
			AddLog("Initialize");
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			Initialized = false;
			try
			{
				ComPortName = ComPort;
				if (comPort != null)
				{
					comPort.Close();
					Thread.Sleep(50);
				}
				comPort = new SerialPort(ComPort, 115200, (Parity)0, 8, (StopBits)1);
				// comPort.set_ReadTimeout(COMTimeoutTime);
				// comPort.set_WriteTimeout(COMTimeoutTime);
                comPort.ReadTimeout = COMTimeoutTime;
				comPort.WriteTimeout = COMTimeoutTime;
				comPort.Open();
				comPort.DiscardInBuffer();
				comPort.DiscardOutBuffer();
				IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("GET_STATUS");
				if (!string.IsNullOrEmpty(Lang))
				{
					iPPProcessor.Add("LANG", Lang);
				}
				StartCommunicationWithTerminal(iPPProcessor);
				return RequestResult.Processing;
			}
			catch (Exception ex)
			{
				AddLog("[Exception] " + ex.Message);
				comPort = null;
				Initialized = false;
				return RequestResult.NotInitialized;
			}
		}

		public void Disconnect()
		{
			try
			{
				if (comPort != null)
				{
                    string portName = comPort.PortName;
					// string portName = comPort.get_PortName();
					comPort.Close();
					AddLog("Port " + portName + " disconnected");
				}
				else
				{
					AddLog("Not connected");
				}
				comPort = null;
				Initialized = false;
			}
			catch (Exception ex)
			{
				AddLog("[Exception] " + ex.Message);
			}
		}

		public void SetLanguage(Language new_language)
		{
			switch (new_language)
			{
			case Language.English:
				Lang = "EN";
				break;
			case Language.Bulgarian:
				Lang = "BG";
				break;
			case Language.Italian:
				Lang = "IT";
				break;
			case Language.Croatian:
				Lang = "HR";
				break;
			case Language.Spanish:
				Lang = "ES";
				break;
			case Language.French:
				Lang = "FR";
				break;
			case Language.German:
				Lang = "DE";
				break;
			case Language.Romanian:
				Lang = "RO";
				break;
			case Language.Greek:
				Lang = "GR";
				break;
			case Language.Dutch:
				Lang = "DU";
				break;
			case Language.Latvian:
				Lang = "LV";
				break;
			case Language.Swedish:
				Lang = "SE";
				break;
			case Language.Portuguese:
				Lang = "PT";
				break;
			case Language.Icelandic:
				Lang = "IS";
				break;
			case Language.Slovenian:
				Lang = "SI";
				break;
			case Language.Hungarian:
				Lang = "HU";
				break;
			case Language.Czech:
				Lang = "CS";
				break;
			case Language.Lithuanian:
				Lang = "LT";
				break;
			case Language.Polish:
				Lang = "PL";
				break;
			case Language.Danish:
				Lang = "DA";
				break;
			case Language.Norwegian:
				Lang = "NO";
				break;
			default:
				Lang = "";
				break;
			}
		}

		public void SetCOMTimeout(int ms)
		{
			if (ms >= 0)
			{
				COMTimeoutTime = ms;
				if (comPort != null)
				{
					//comPort.set_ReadTimeout(COMTimeoutTime);
					//comPort.set_WriteTimeout(COMTimeoutTime);
                    comPort.ReadTimeout = COMTimeoutTime;
					comPort.WriteTimeout = COMTimeoutTime;
				}
			}
		}

		public void SetPassword(string password)
		{
			Password = password;
		}

		public void AbortOperation()
		{
			Abort = true;
			HostClose();
		}

		public RequestResult GetStatus()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("GET_STATUS");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		private bool IsValidAmount(double amount, Currencies currency)
		{
			if (amount < 0.01)
			{
				return false;
			}
			if (currency == Currencies.ISK && amount > 9999999.0)
			{
				return false;
			}
			if (amount > 999999.99)
			{
				return false;
			}
			return true;
		}

		private bool IsValidMoToParams(string PAN, string ExpiryDate)
		{
			if (!Regex.IsMatch(PAN, "[0-9]{13,19}"))
			{
				return false;
			}
			if (ExpiryDate.Length > 0)
			{
				if (!Regex.IsMatch(ExpiryDate, "[0-9]{4}"))
				{
					return false;
				}
				if (!int.TryParse(ExpiryDate.Substring(0, 2), out var result))
				{
					return false;
				}
				if (result < 1 || result > 12)
				{
					return false;
				}
			}
			return true;
		}

		public RequestResult Purchase(double amount, Currencies currency, string reference)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("PURCHASE");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("REFERENCE", reference);
			iPPProcessor.Add("FIXED_PINPAD", $"{(isFixedPinpad ? 1 : 0)}");
			if (receipt_mode != ReceiptMode.NotConfugred)
			{
				iPPProcessor.Add("PRINT_RECEIPT", $"{(int)receipt_mode}");
			}
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult Purchase(double amount, double tip, Currencies currency, ReferenceNumberType ref_type, string reference_number, string operator_code)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("PURCHASE");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("FIXED_PINPAD", $"{(isFixedPinpad ? 1 : 0)}");
			if (tip > 0.0)
			{
				iPPProcessor.Add("TIP_AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { tip }));
			}
			if (ref_type != 0)
			{
				iPPProcessor.Add("REFERENCE_NUMBER_TYPE", $"{(int)ref_type}");
				iPPProcessor.Add("REFERENCE_NUMBER", reference_number);
			}
			if (!string.IsNullOrEmpty(operator_code))
			{
				iPPProcessor.Add("OPERATOR_CODE", operator_code);
			}
			if (receipt_mode != ReceiptMode.NotConfugred)
			{
				iPPProcessor.Add("PRINT_RECEIPT", $"{(int)receipt_mode}");
			}
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult Refund(double amount, Currencies currency, string reference)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("REFUND");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("FIXED_PINPAD", $"{(isFixedPinpad ? 1 : 0)}");
			iPPProcessor.Add("REFERENCE", reference);
			if (receipt_mode != ReceiptMode.NotConfugred)
			{
				iPPProcessor.Add("PRINT_RECEIPT", $"{(int)receipt_mode}");
			}
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			if (!string.IsNullOrEmpty(Password))
			{
				iPPProcessor.Add("PASSWORD", Password);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult Reversal(string reference)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("REVERSAL");
			iPPProcessor.Add("SID_ORIGINAL", LastTransactionSID);
			iPPProcessor.Add("REFERENCE", reference);
			if (receipt_mode != ReceiptMode.NotConfugred)
			{
				iPPProcessor.Add("PRINT_RECEIPT", $"{(int)receipt_mode}");
			}
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			if (!string.IsNullOrEmpty(Password))
			{
				iPPProcessor.Add("PASSWORD", Password);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult Preauthorization(double amount, Currencies currency, string reference)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("PURCHASE");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("REFERENCE", reference);
			iPPProcessor.Add("PREAUTH_TX", "1");
			iPPProcessor.Add("FIXED_PINPAD", $"{(isFixedPinpad ? 1 : 0)}");
			if (receipt_mode != ReceiptMode.NotConfugred)
			{
				iPPProcessor.Add("PRINT_RECEIPT", $"{(int)receipt_mode}");
			}
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult CompletePreauth(string PreauthCode, double amount)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (amount <= 0.0)
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("COMPLETE_PREAUTH");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("PREAUTH_CODE", PreauthCode);
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult CancelPreauth(string PreauthCode)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("CANCEL_PREAUTH");
			iPPProcessor.Add("PREAUTH_CODE", PreauthCode);
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult MoToPurchase(double amount, Currencies currency, string PAN, string ExpiryDate, string reference)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			if (!IsValidMoToParams(PAN, ExpiryDate))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("PURCHASE");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("REFERENCE", reference);
			iPPProcessor.Add("MOTO_TX", "1");
			iPPProcessor.Add("PAN", PAN);
			iPPProcessor.Add("EXP_DATE", ExpiryDate);
			if (receipt_mode != ReceiptMode.NotConfugred)
			{
				iPPProcessor.Add("PRINT_RECEIPT", $"{(int)receipt_mode}");
			}
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			if (!string.IsNullOrEmpty(Password))
			{
				iPPProcessor.Add("PASSWORD", Password);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult MoToRefund(double amount, Currencies currency, string PAN, string ExpiryDate, string reference)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			if (!IsValidMoToParams(PAN, ExpiryDate))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("REFUND");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("REFERENCE", reference);
			iPPProcessor.Add("MOTO_TX", "1");
			iPPProcessor.Add("PAN", PAN);
			iPPProcessor.Add("EXP_DATE", ExpiryDate);
			if (receipt_mode != ReceiptMode.NotConfugred)
			{
				iPPProcessor.Add("PRINT_RECEIPT", $"{(int)receipt_mode}");
			}
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			if (!string.IsNullOrEmpty(Password))
			{
				iPPProcessor.Add("PASSWORD", Password);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult MoToPreauthorization(double amount, Currencies currency, string PAN, string ExpiryDate, string reference)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			if (!IsValidMoToParams(PAN, ExpiryDate))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("PURCHASE");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("REFERENCE", reference);
			iPPProcessor.Add("PREAUTH_TX", "1");
			iPPProcessor.Add("MOTO_TX", "1");
			iPPProcessor.Add("PAN", PAN);
			iPPProcessor.Add("EXP_DATE", ExpiryDate);
			if (receipt_mode != ReceiptMode.NotConfugred)
			{
				iPPProcessor.Add("PRINT_RECEIPT", $"{(int)receipt_mode}");
			}
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult Update()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("UPDATE");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult Activate()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("ACTIVATE");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult Deactivate()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("DEACTIVATE");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult ReprintReceipt()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("REPRINT_RECEIPT");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult PrintExternalWithEncoding(string PrintData, Encoding enc)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("PRINT_EXT");
			string text = (string)PrintData.Clone();
			for (int num = text.IndexOf('\\'); num >= 0; num = text.IndexOf('\\', num + 1))
			{
				if (num + 1 == text.Length)
				{
					return RequestResult.InvalidParams;
				}
				switch (text[num + 1])
				{
				case '\\':
					text = text.Remove(num, 1);
					break;
				case 'n':
					text = text.Remove(num, 2).Insert(num, "\n");
					break;
				case 'L':
					text = text.Remove(num, 2).Insert(num, "\f031");
					break;
				case 'l':
					text = text.Remove(num, 2).Insert(num, "\f021");
					break;
				case 'c':
					text = text.Remove(num, 2).Insert(num, "\f022");
					break;
				case 'r':
					text = text.Remove(num, 2).Insert(num, "\f023");
					break;
				case 'w':
					text = text.Remove(num, 2).Insert(num, "\f011");
					break;
				case 'h':
					text = text.Remove(num, 2).Insert(num, "\f012");
					break;
				case 'W':
					text = text.Remove(num, 2).Insert(num, "\f013");
					break;
				case 'H':
					text = text.Remove(num, 2).Insert(num, "\f014");
					break;
				default:
					AddLog("[Print] Unknown escape in print data: " + text.Substring(num, 2));
					return RequestResult.InvalidParams;
				}
			}
			text = text.Replace("\r\n", "\n");
			text += "\n";
			iPPProcessor.Add("PRINT_DATA", enc.GetBytes(text));
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult PrintExternal(string PrintData)
		{
			Encoding enc = Encoding.ASCII;
			switch (Lang)
			{
			case "HR":
			case "RO":
			case "SI":
			case "CS":
			case "PL":
			case "HU":
				enc = Encoding.GetEncoding(1250);
				break;
			case "EN":
			case "BG":
			case "IT":
				enc = Encoding.GetEncoding(1251);
				break;
			case "ES":
			case "DE":
			case "FR":
			case "DU":
			case "SE":
			case "PT":
			case "IS":
			case "DA":
			case "NO":
				enc = Encoding.GetEncoding(1252);
				break;
			case "GR":
				enc = Encoding.GetEncoding(1253);
				break;
			case "LV":
			case "LT":
				enc = Encoding.GetEncoding(1253);
				break;
			}
			return PrintExternalWithEncoding(PrintData, enc);
		}

		public RequestResult PrintExternalUTF8(string PrintData)
		{
			return PrintExternalWithEncoding(PrintData, Encoding.UTF8);
		}

		public RequestResult Ping()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("PING");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult Reboot()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("REBOOT");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public void SetReceiptMode(ReceiptMode mode)
		{
			receipt_mode = mode;
		}

		public RequestResult GiftcardActivation(double amount, Currencies currency)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("GIFTCARD_ACTIVATION");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult GiftcardDeactivation()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("GIFTCARD_DEACTIVATION");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult GiftcardCheckBalance()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("GIFTCARD_CHECK_BALANCE");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult SendPaymentRequest(double amount, Currencies currency, string GSM, string EMail, string RecipientName, string Reason, int DaysValid)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("PAYMENT_REQUEST");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("RECIPIENT_GSM", GSM);
			iPPProcessor.Add("RECIPIENT_EMAIL", EMail);
			iPPProcessor.Add("RECIPIENT_NAME", RecipientName);
			iPPProcessor.Add("REQUEST_REASON", Reason);
			iPPProcessor.Add("EXP_DAYS", $"{DaysValid}");
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult CheckCard()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("CHECK_CARD");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult OriginalCredit(double amount, Currencies currency)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("ORIGINAL_CREDIT");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult SendLog()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("SEND_LOG");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult VendingPurchase(double amount, Currencies currency)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("VENDING_PURCHASE");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("FIXED_PINPAD", $"{(isFixedPinpad ? 1 : 0)}");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult VendingComplete(double amount, Currencies currency)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("COMPLETE_TX");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("FIXED_PINPAD", $"{(isFixedPinpad ? 1 : 0)}");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult VendingCancel()
		{
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("CANCEL_TX");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult VendingStop()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("VENDING_PURCHASE");
			iPPProcessor.Add("STAGE", "101");
			SendRequestToTerminal(iPPProcessor);
			return RequestResult.Finished;
		}

		public RequestResult Beep(BeepTone tone, int duration)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("BEEP");
			iPPProcessor.Add("TONE_TYPE", $"{(int)tone}");
			iPPProcessor.Add("TONE_DURATION", $"{duration}");
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult OpenSettings()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor request = IPPProcessor.CreateRequest("OPEN_SETTINGS");
			return StartCommunicationWithTerminal(request);
		}

		public RequestResult CheckForCRRTransaction()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			IPPProcessor request = IPPProcessor.CreateRequest("CHECK_TRANSACTION");
			return StartCommunicationWithTerminal(request);
		}

		public RequestResult StopWaitingForCard()
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			switch (LastUsedMethod)
			{
			default:
				return RequestResult.InvalidParams;
			case Method.PURCHASE:
			case Method.REFUND:
			case Method.VENDING_PURCHASE:
			case Method.CASH_ADVANCE:
			{
				string lastReceivedStage = LastReceivedStage;
				if (!(lastReceivedStage == "1") && !(lastReceivedStage == "11"))
				{
					return RequestResult.Busy;
				}
				IPPProcessor iPPProcessor = IPPProcessor.CreateRequest(LastUsedMethod.ToString());
				iPPProcessor.Add("STAGE", "101");
				SendRequestToTerminal(iPPProcessor);
				return RequestResult.Finished;
			}
			}
		}

		public RequestResult CashAdvance(double amount, Currencies currency)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("CASH_ADVANCE");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("FIXED_PINPAD", $"{(isFixedPinpad ? 1 : 0)}");
			if (receipt_mode != ReceiptMode.NotConfugred)
			{
				iPPProcessor.Add("PRINT_RECEIPT", $"{(int)receipt_mode}");
			}
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}

		public RequestResult VendingPurchase(double amount, Currencies currency, bool bShowAmountOnScreen)
		{
			if (!Initialized)
			{
				return RequestResult.NotInitialized;
			}
			if (!IsValidAmount(amount, currency))
			{
				return RequestResult.InvalidParams;
			}
			IPPProcessor iPPProcessor = IPPProcessor.CreateRequest("VENDING_PURCHASE");
			iPPProcessor.Add("AMOUNT", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[1] { amount }));
			iPPProcessor.Add("CURRENCY", $"{(int)currency}");
			iPPProcessor.Add("FIXED_PINPAD", $"{(isFixedPinpad ? 1 : 0)}");
			iPPProcessor.Add("SHOW_AMOUNT", $"{(bShowAmountOnScreen ? 1 : 0)}");
			if (!string.IsNullOrEmpty(Lang))
			{
				iPPProcessor.Add("LANG", Lang);
			}
			return StartCommunicationWithTerminal(iPPProcessor);
		}
	}
}
