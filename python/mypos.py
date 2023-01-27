from __future__ import annotations
from typing import Tuple
from enum import Enum
import uuid
import sys

if sys.implementation.name == 'micropython':
    from micropython import const

if sys.implementation.name == 'micropython':
    class BeepTone(Enum):
        TONE_1680_Hz = const(0),
        TONE_1850_Hz = const(1),
        TONE_2020_Hz = const(2),
        TONE_2130_Hz = const(3),
        TONE_2380_Hz = const(4),
        TONE_2700_Hz = const(5),
        TONE_2750_Hz = const(6),
else:
    class BeepTone(Enum):
        TONE_1680_Hz = 0,
        TONE_1850_Hz = 1,
        TONE_2020_Hz = 2,
        TONE_2130_Hz = 3,
        TONE_2380_Hz = 4,
        TONE_2700_Hz = 5,
        TONE_2750_Hz = 6,

if sys.implementation.name == 'micropython':
    class Currencies:
        EUR = const(978),
        BGN = const(975),
        CHF = const(756),
        GBP = const(826),
        RON = const(946),
        USD = const(840),
        HRK = const(191),
        JPY = const(392),
        CZK = const(203),
        DKK = const(208),
        HUF = const(348),
        ISK = const(352),
        NOK = const(578),
        SEK = const(752),
        PLN = const(985),
else:
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

if sys.implementation.name == 'micropython':
    class Language:
        English = const(0),
        Bulgarian = const(1),
        Italian = const(2),
        Croatian = const(3),
        Spanish = const(4),
        French = const(5),
        German = const(6),
        Romanian = const(7),
        Greek = const(8),
        Dutch = const(9),
        Latvian = const(10),
        Swedish = const(11),
        Portuguese = const(12),
        Icelandic = const(13),
        Slovenian = const(14),
        Hungarian = const(15),
        Czech = const(16),
        Lithuanian = const(17),
        Polish = const(18),
        Danish = const(19),
        Norwegian = const(20),
else:
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

if sys.implementation.name == 'micropython':
    class Method:
        none = const(0),
        PURCHASE = const(1),
        COMPLETE_TX = const(2),
        CANCEL_TX = const(3),
        COMPLETE_PREAUTH = const(4),
        CANCEL_PREAUTH = const(5),
        REVERSAL = const(6),
        REFUND = const(7),
        GET_STATUS = const(8),
        PRINT_EXT = const(9),
        ACTIVATE = const(10),
        UPDATE = const(11),
        REPRINT_RECEIPT = const(12),
        DEACTIVATE = const(13),
        GET_CERTIFICATE = const(14),
        PING = const(15),
        REBOOT = const(16),
        GIFTCARD_ACTIVATION = const(17),
        GIFTCARD_DEACTIVATION = const(18),
        GIFTCARD_CHECK_BALANCE = const(19),
        PAYMENT_REQUEST = const(20),
        SEND_LOG = const(21),
        VENDING_PURCHASE = const(22),
        BEEP = const(23),
        OPEN_SETTINGS = const(24),
        CHECK_TRANSACTION = const(25),
        PROCESS = const(26),
        CASH_ADVANCE = const(27),
        CHECK_CARD = const(10001),
        ORIGINAL_CREDIT = const(10002),
else:
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
    
if sys.implementation.name == 'micropython':
    class TransactionStatus:
        Success = const(0),
        Busy = const(1),
        SyntaxError = const(2),
        UnsupportedParam = const(3),
        UnsupportedMethod = const(4),
        NoCardFound = const(5),
        UnsupportedCard = const(6),
        CardChipError = const(7),
        FallbackToMagstripe = const(8),
        InvalidPin = const(9),
        PinCountLimitExceeded = const(10),
        PinCheckOnline = const(11),
        InvalidDataFromHost = const(12),
        UserCancel = const(13),
        InternalError = const(14),
        CommunicationError = const(15),
        SSLError = const(16),
        TransactionNotFound = const(17),
        ReversalNotFound = const(18),
        InvalidAmount = const(19),
        LastTransactionIncomplete = const(20),
        NoPrinterAvailable = const(21),
        NoPaper = const(22),
        IncorrectDataForPrint = const(23),
        IncorrectLogoIndex = const(24),
        ActivationRequired = const(25),
        MandatoryUpdateRequired = const(26),
else:
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
    
if sys.implementation.name == 'micropython':
    class ResultCommunication:
        Finished = const(0),
        ExpectAnotherMessage = const(1),
        Error = const(2),
        Timeout = const(3),
        Aborted = const(4),
else:
    class ResultCommunication(Enum):
        Finished = 0,
        ExpectAnotherMessage = 1,
        Error = 2,
        Timeout = 3,
        Aborted = 4,


if sys.implementation.name == 'micropython':
    class RequestResult:
        Finished = const(0),
        Processing = const(1),
        InvalidParams = const(2),
        Busy = const(3),
        NotInitialized = const(4),
else:    
    class RequestResult(Enum):
        Finished = 0,
        Processing = 1,
        InvalidParams = 2,
        Busy = 3,
        NotInitialized = 4,    
if sys.implementation.name == 'micropython':
    class ReferenceNumberType:
        none = const(0),
        ReferenceNumber = const(1),
        InvoiceNumber = const(2),
        ProductID = const(3),
        ReservationNumber = const(4),
else:    
    class ReferenceNumberType(Enum):
        none = 0,
        ReferenceNumber = 1,
        InvoiceNumber = 2,
        ProductID = 3,
        ReservationNumber = 4,    
    
if sys.implementation.name == 'micropython':
        class ReceiptMode:
            NotConfugred = const(-1),
            Automatic = const(0),
            AfterConfirmation = const(1),
            MerchantOnly = const(2),
            NoReceipt = const(3),
else:
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
        # Initialize the result variable
        result = 0
        # Check that the PROTOCOL field is present and has the value "IPP"
        field = self.Get("PROTOCOL")
        if field is None or field.str_data != "IPP":
            return False
        # Check that the METHOD field is present and has a non-empty value
        field = self.Get("METHOD")
        if field is None or field.str_data == "":
            return False
        # Check that the SID field is present and has a non-empty value
        field = self.Get("SID")
        if field is None or field.str_data == "":
            return False
        # Check that the VERSION field is present
        field = self.Get("VERSION")
        if field is None:
            return False
        # Try to convert the VERSION field to an integer and check that it is greater than or equal to 200
        try:
            result = int(field.str_data)
        except ValueError:
            return False
        if result < 200:
            return False
        # If all the above checks pass, return True
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
        # append length of list at position 0
        list.append(0)
        # append length of list at position 1
        list.append(0)
        # iterating over fields
        for i in range(len(self.fields)):
            # checking if field is not None
            if self.fields[i] != None:
                # checking if field name is one of these
                if self.fields[i].name in ["PRIMARY_CHAIN", "SECONDARY_CHAIN", "FINGERPRINT", "CHAIN", "PRINT_DATA"]:
                    # appending field name
                    list += list(self.fields[i].name.encode('ascii'))
                    # appending '='
                    list.append(61)
                    # appending field binary data
                    list += self.fields[i].bin_data
                    # appending new line
                    list += [13, 10]
                # checking if field name is DATA
                elif self.fields[i].name == "DATA":
                    # appending field name
                    list += list(self.fields[i].name.encode('ascii'))
                    # appending '='
                    list.append(61)
                    # appending size of binary data
                    list.append(self.fields[i].bin_data_size // 256)
                    list.append(self.fields[i].bin_data_size % 256)
                    # appending field binary data
                    list += self.fields[i].bin_data
                    # appending new line
                    list += [13, 10]
                else:
                    # appending field name
                    list += list(self.fields[i].name.encode('ascii'))
                    # appending '='
                    list.append(61)
                    # appending field string data
                    list += list(self.fields[i].str_data.encode('ascii'))
                    # appending new line
                    list += [13, 10]
        # update length of list at position 0
        list[0] = len(list) // 256
        # update length of list at position 1
        list[1] = len(list) % 256
        # returning the list in bytes format
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
                return False, output  # If pos is greater than or equal to the length of data, return
            field.name = data[start:pos].decode()
            pos += 1
            if field.name in ["DATA", "CERT"]:
                if pos + 2 >= len(data):
                    # If the length of data is less than the current position plus 2, return False and output
                    return False, output
                length = (data[pos] << 8) + data[pos + 1]
                start = pos
                if pos + length > len(data):
                    # If the current position plus length is greater than the length of data, return False and output
                    return False, output
                pos += length
                if data[pos] != 13 or data[pos + 1] != 10:
                    return False, output  # If the current position in data is not equal to 13 or the next position is not equal to 10, return False and output
                pos += 2
                field.bin_data_size = length
                field.bin_data = data[start:start+length]
            elif field.name in ["PRIMARY_CHAIN", "SECONDARY_CHAIN", "CHAIN"]:
                start = pos
                while data[pos] != 13 or data[pos + 1] != 10:  # CRLF
                    pos += 20  # 20 bytes per fingerprint
                    if pos >= len(data):
                        # If the current position is greater than or equal to the length of data, return False and output
                        return False, output
                    if data[pos] != 59:  # ;
                        # If the current position in data is not equal to 59 (;), return False and output
                        return False, output
                    pos += 1
                    if pos >= len(data):
                        break
                if pos >= len(data):
                    # If the current position is greater than or equal to the length of data, return False and output
                    return False, output
                length = pos - start
                pos += 2
                if length > 0:
                    field.bin_data_size = length
                field.bin_data = data[start:start+length]
            elif field.name == "FINGERPRINT":
                start = pos
                pos += 20  # 20 bytes per fingerprint
                if pos >= len(data):
                    # If the current position is greater than or equal to the length of data, return False and output
                    return False, output
                if data[pos] != 13 or data[pos + 1] != 10:  # CRLF
                    # If the current position in data is not equal to CRLF, return False and output
                    return False, output
                length = pos - start
                pos += 2
                field.bin_data_size = length
                field.bin_data = data[start:start+length]
            else:
                start = pos
                # CRLF
                while pos < len(data) - 1 and (data[pos] != 13 or data[pos + 1] != 10):
                    pos += 1
                if pos >= len(data):
                    # If the current position is greater than or equal to the length of data, return False and output
                    return False, output
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
