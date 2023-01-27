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
    CommunicationError = 15,
    SSLError = 16,
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
    SoftwareUpdateWaitHost = 31,
    SoftwareUpdateDontWaitHost = 32,
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
        #append length of list at position 0
        list.append(0)
        #append length of list at position 1
        list.append(0)
        #iterating over fields
        for i in range(len(self.fields)):
            #checking if field is not None
            if self.fields[i] != None:
                #checking if field name is one of these
                if self.fields[i].name in ["PRIMARY_CHAIN", "SECONDARY_CHAIN", "FINGERPRINT", "CHAIN", "PRINT_DATA"]:
                    #appending field name
                    list += list(self.fields[i].name.encode('ascii'))
                    #appending '='
                    list.append(61)
                    #appending field binary data
                    list += self.fields[i].bin_data
                    #appending new line
                    list += [13, 10]
                #checking if field name is DATA
                elif self.fields[i].name == "DATA":
                    #appending field name
                    list += list(self.fields[i].name.encode('ascii'))
                    #appending '='
                    list.append(61)
                    #appending size of binary data
                    list.append(self.fields[i].bin_data_size // 256)
                    list.append(self.fields[i].bin_data_size % 256)
                    #appending field binary data
                    list += self.fields[i].bin_data
                    #appending new line
                    list += [13, 10]
                else:
                    #appending field name
                    list += list(self.fields[i].name.encode('ascii'))
                    #appending '='
                    list.append(61)
                    #appending field string data
                    list += list(self.fields[i].str_data.encode('ascii'))
                    #appending new line
                    list += [13, 10]
        #update length of list at position 0
        list[0] = len(list) // 256
        #update length of list at position 1
        list[1] = len(list) % 256
        #returning the list in bytes format
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
        pos = 0
        start = 0
        length = 0
        while pos < len(data):
            field = Field()
            start = pos
            pos += 1
            while pos < len(data) and data[pos] != 61:
                pos += 1
            if pos >= len(data):
                return False, output # If pos is greater than or equal to the length of data, return 
            field.name = data[start:pos].decode()
            pos += 1
            if field.name in ["DATA", "CERT"]:
                if pos + 2 >= len(data):
                    return False, output # If the length of data is less than the current position plus 2, return False and output
                length = (data[pos] << 8) + data[pos + 1]
                start = pos
                if pos + length > len(data):
                    return False, output # If the current position plus length is greater than the length of data, return False and output
                pos += length
                if data[pos] != 13 or data[pos + 1] != 10:
                    return False, output # If the current position in data is not equal to 13 or the next position is not equal to 10, return False and output
                pos += 2
                field.bin_data_size = length
                field.bin_data = data[start:start+length]
            elif field.name in ["PRIMARY_CHAIN", "SECONDARY_CHAIN", "CHAIN"]:
                start = pos
                while data[pos] != 13 or data[pos + 1] != 10: # CRLF
                    pos += 20 # 20 bytes per fingerprint
                    if pos >= len(data):
                        return False, output # If the current position is greater than or equal to the length of data, return False and output
                    if data[pos] != 59: # ;
                        return False, output  #If the current position in data is not equal to 59 (;), return False and output
                    pos += 1
                    if pos >= len(data):
                        break
                if pos >= len(data):
                    return False, output # If the current position is greater than or equal to the length of data, return False and output
                length = pos - start
                pos += 2
                if length > 0:
                    field.bin_data_size = length
                field.bin_data = data[start:start+length]
            elif field.name == "FINGERPRINT":
                start = pos
                pos += 20 # 20 bytes per fingerprint
                if pos >= len(data):
                    return False, output # If the current position is greater than or equal to the length of data, return False and output
                if data[pos] != 13 or data[pos + 1] != 10: # CRLF
                    return False, output # If the current position in data is not equal to CRLF, return False and output
                length = pos - start
                pos += 2
                field.bin_data_size = length
                field.bin_data = data[start:start+length]
            else:
                start = pos
                while pos < len(data) - 1 and (data[pos] != 13 or data[pos + 1] != 10): # CRLF
                    pos += 1
                if pos >= len(data):
                    return False, output # If the current position is greater than or equal to the length of data, return False and output
                length = pos - start
                pos += 2
                field.bin_data_size = length
                field.bin_data = data[start:start+length]
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


