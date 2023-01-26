from enum import Enum


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


class IPPProcessor:
    def __init__(self):
        self.fields = []

    def get(self, name):
        for field in self.fields:
            if field.name == name:
                return field
        field = Field()
        field.name = name
        fields.append(Field())
        return None
