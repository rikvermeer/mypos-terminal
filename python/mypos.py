from __future__ import annotations
from typing import Tuple
from enum import Enum
import uuid


class BeepTone(Enum):
    TONE_1680_Hz = 0,
    TONE_1850_Hz = 1,
    TONE_2020_Hz = 2,
    TONE_2130_Hz = 3,
    TONE_2380_Hz = 4,
    TONE_2700_Hz = 5,
    TONE_2750_Hz = 6,


class Currencies(Enum):
    EUR = 978,
    BGN = 975,
    CHF = 756,
    GBP = 826,
    RON = 946,
    USD = 840,
    HRK = 191,
    JPY = 392,
    CZK = 203,
    DKK = 208,
    HUF = 348,
    ISK = 352,
    NOK = 578,
    SEK = 752,
    PLN = 985


class Language(Enum):
    English = 0,
    Bulgarian = 1,
    Italian = 2,
    Croatian = 3,
    Spanish = 4,
    French = 5,
    German = 6,
    Romanian = 7,
    Greek = 8,
    Dutch = 9,
    Latvian = 10,
    Swedish = 11,
    Portuguese = 12,
    Icelandic = 13,
    Slovenian = 14,
    Hungarian = 15,
    Czech = 16,
    Lithuanian = 17,
    Polish = 18,
    Danish = 19,
    Norwegian = 20


class Method(Enum):
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
    PING = 15,
    REBOOT = 16,
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


class TransactionStatus(Enum):
    Success = 0,
    Busy = 1,
    SyntaxError = 2,
    UnsupportedParam = 3,
    UnsupportedMethod = 4,
    NoCardFound = 5,
    UnsupportedCard = 6,
    CardChipError = 7,
    FallbackToMagstripe = 8,
    InvalidPin = 9,
    PinCountLimitExceeded = 10,
    PinCheckOnline = 11,
    InvalidDataFromHost = 12,
    UserCancel = 13,
    InternalError = 14,
    CommunicationError = 0xF,
    SSLError = 0x10,
    TransactionNotFound = 17,
    ReversalNotFound = 18,
    InvalidAmount = 19,
    LastTransactionIncomplete = 20,
    NoPrinterAvailable = 21,
    NoPaper = 22,
    IncorrectDataForPrint = 23,
    IncorrectLogoIndex = 24,
    ActivationRequired = 25,
    MandatoryUpdateRequired = 26,
    TerminalAlreadyActive = 27,
    ActivationUnsuccessful = 28,
    NoUpdateFound = 29,
    SoftwareUpdateUnsuccessful = 30,
    SoftwareUpdateWaitHost = 0x1F,
    SoftwareUpdateDontWaitHost = 0x20,
    DeactivationUnsuccessful = 33,
    OptionalUpdateRequired = 34,
    WrongCode = 35,
    DeactivationNotFinished = 36,
    FingerprintNotFound = 37,
    UnsupportedProtocolVersion = 38,
    WrongPreauthCode = 39,
    PreauthCompleted = 40,
    RemotelyActivated = 41,
    IvalidPAN = 47,
    InvalidExpiryDate = 48,
    WrongPassword = 49,
    SuccessWithInfo = 100,
    Timeout = -1

class ResultCommunication(Enum):
    Finished = 0,
    ExpectAnotherMessage = 1,
    Error = 2,
    Timeout = 3,
    Aborted = 4,

class RequestResult(Enum):
    Finished = 0,
    Processing = 1,
    InvalidParams = 2,
    Busy = 3,
    NotInitialized = 4,

class ReferenceNumberType(Enum):
    none = 0,
    ReferenceNumber = 1,
    InvoiceNumber = 2,
    ProductID = 3,
    ReservationNumber = 4,

class ReceiptMode(Enum):
    NotConfugred = -1,
    Automatic = 0,
    AfterConfirmation = 1,
    MerchantOnly = 2,
    NoReceipt = 3,

class ProcessingResult:
    def __init__(self):
        self.Method = Method.none
        self.Status = TransactionStatus.TransactionNotFound
        self.TranData = TransactionData()



class TransactionData:
    def __init__(self):
        self.Type = ""
        self.AuthCode = ""
        self.Approval = ""
        self.TransactionDate = ""
        self.RRN = ""
        self.Amount = ""
        self.TipAmount = ""
        self.Currency = ""
        self.TerminalID = ""
        self.MerchantID = ""
        self.MerchantName = ""
        self.MerchantAddressLine1 = ""
        self.MerchantAddressLine2 = ""
        self.PANMasked = ""
        self.PreauthCode = ""
        self.EmbossName = ""
        self.AID = ""
        self.AIDName = ""
        self.ApplicationPreferredName = ""
        self.Stan = ""
        self.SignatureRequired = False
        self.SoftwareVersion = ""


class DCCRequest:
    def __init__(self):
        self.OriginalAmount = ""
        self.OriginalCurrencyCode = ""
        self.OriginalCurrencyName = ""
        self.DCCAmount = ""
        self.DCCCurrencyCode = ""
        self.DCCCurrencyName = ""
        self.DCCExchangeRate = ""


class Field:
    def __init__(self):
        self.name = ""
        self.str_data = ""
        self.bin_data_size = 0
        self.bin_data = b""
    
    @classmethod
    def FromString(cls, name, data):
        field = cls()
        field.name = name
        field.str_data = data
        return field


class IPPProcessor:
    def __init__(self):
        self.fields = []

    def IsValid(self):
        result = 0
        field = self.Get("PROTOCOL")
        if field is None or field.str_data != "IPP":
            return False
        field = self.Get("METHOD")
        if field is None or field.str_data == "":
            return False
        field = self.Get("SID")
        if field is None or field.str_data == "":
            return False
        field = self.Get("VERSION")
        if field is None:
            return False
        try:
            result = int(field.str_data)
        except ValueError:
            return False
        if result < 200:
            return False
        return True

    def CreateResult(self, status):
        result = IPPProcessor()
        result.Add(self.Get("PROTOCOL"))
        result.Add(self.Get("VERSION"))
        result.Add(self.Get("METHOD"))
        result.Add(self.Get("SID"))
        result.Add("STATUS", str(status))
        result.Add(self.Get("STAGE"))
        return result

    def GetDataForSending(self):
        list = []
        list.append(0)
        list.append(0)
        for i in range(len(self.fields)):
            if self.fields[i] != None:
                if self.fields[i].name in ["PRIMARY_CHAIN", "SECONDARY_CHAIN", "FINGERPRINT", "CHAIN", "PRINT_DATA"]:
                    list += list(self.fields[i].name.encode('ascii'))
                    list.append(61)
                    list += self.fields[i].bin_data
                    list += [13, 10]
                elif self.fields[i].name == "DATA":
                    list += list(self.fields[i].name.encode('ascii'))
                    list.append(61)
                    list.append(self.fields[i].bin_data_size // 256)
                    list.append(self.fields[i].bin_data_size % 256)
                    list += self.fields[i].bin_data
                    list += [13, 10]
                else:
                    list += list(self.fields[i].name.encode('ascii'))
                    list.append(61)
                    list += list(self.fields[i].str_data.encode('ascii'))
                    list += [13, 10]
        list[0] = len(list) // 256
        list[1] = len(list) % 256
        return bytes(list)

    @classmethod
    def CreateRequest(cls, method: str) -> IPPProcessor:
        iPPProcessor = IPPProcessor()
        iPPProcessor.fields = [
            Field.FromString("PROTOCOL", "IPP"),
            Field.FromString("VERSION", "200"),
            Field.FromString("METHOD", method),
            Field.FromString("SID", uuid.uuid4().hex),
        ]
        return iPPProcessor

    @classmethod
    def TryParse(cls, data: bytes) -> Tuple[bool, IPPProcessor]:
        output = None
        iPPProcessor = IPPProcessor()
        num = 0
        num2 = 0
        num3 = 0
        while num < len(data):
            field = Field()
            num2 = num
            num += 1
            while num < len(data) and data[num] != 61:
                num += 1
            if num >= len(data):
                return False, output
            field.name = data[num2:num].decode()
            num += 1
            if field.name in ["DATA", "CERT"]:
                if num + 2 >= len(data):
                    return False, output
                num3 = (data[num] << 8) + data[num + 1]
                num2 = num
                if num + num3 > len(data):
                    return False, output
                num += num3
                if data[num] != 13 or data[num + 1] != 10:
                    return False, output
                num += 2
                field.bin_data_size = num3
                field.bin_data = data[num2:num2+num3]
            elif field.name in ["PRIMARY_CHAIN", "SECONDARY_CHAIN", "CHAIN"]:
                num2 = num
                while data[num] != 13 or data[num + 1] != 10:
                    num += 20
                    if num >= len(data):
                        return False, output
                    if data[num] != 59:
                        return False, output
                    num += 1
                    if num >= len(data):
                        break
                if num >= len(data):
                    return False, output
                num3 = num - num2
                num += 2
                if num3 > 0:
                    field.bin_data_size = num3
                field.bin_data = data[num2:num2+num3]
            elif field.name == "FINGERPRINT":
                num2 = num
                num += 20
                if num >= len(data):
                    return False, output
                if data[num] != 13 or data[num + 1] != 10:
                    return False, output
                num3 = num - num2
                num += 2
                field.bin_data_size = num3
                field.bin_data = data[num2:num2+num3]
            else:
                num2 = num
                while num < len(data) - 1 and (data[num] != 13 or data[num + 1] != 10):
                    num += 1
                if num >= len(data):
                    return False, output
                num3 = num - num2
                num += 2
                field.bin_data_size = num3
                field.bin_data = data[num2:num2+num3]
            iPPProcessor.fields.append(field)
        return True, iPPProcessor


    def get(self, name):
        for field in self.fields:
            if field.name == name:
                return field
        field = Field()
        field.name = name
        self.fields.append(Field())
        return None

    def add(self, name, param):
        field = Field()
        field.name = name
        if type(param) == str:
            field.str_data = param
        else:
            field.bin_data = param
            field.bin_data_size = len(param)
        self.fields.append(field)

    def addField(self, field):
        self.fields.append(field)


