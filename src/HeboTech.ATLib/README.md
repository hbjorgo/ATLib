## About
ATLib is a C# AT command library that abstracts away the commands and makes it easy to communicate with modems.

Hayes command set (commonly known as AT commands) is a command set frequently used in modems. Read more about it at [Wikipedia](https://en.wikipedia.org/wiki/Hayes_command_set).

## Supported commands:
- Send SMS in PDU format (GSM 7 bit or UCS2 encoding)
- Send concatenated SMS (message that spans over multiple SMSs) in PDU format (GSM 7 bit or UCS2 encoding)
- SMS supports emojies
- List SMSs
- Read SMS (PDU format (GSM 7 bit or UCS2 encoding))
- Delete SMS
- Get SMS Status Report (delivery status)
- Dial number
- Answer incoming call
- Hang up call
- Get SIM status
- Enter SIM PIN
- Get remaining PIN & PUK attempts
- Get product information
- Get battery status
- Get signal strength
- Get / set date and time
- Disable echo
- Send USSD code
- Get / set character set
- Get IMSI
- Some modems may also support modem specific commands

## Events
- Incoming call
- Missed call
- Call started
- Call ended
- SMS received
- SMS Status Report received
- Error received
- USSD response received
- Generic event

## Supported modems:
- Adafruit FONA 3G (based on SIMCOM SIM5320 chipset)
- D-Link DWM-222 (based on Qualcomm MDM9225 chipset)
- TP-LINK MA260 (based on a Qualcomm chipset)
- Cinterion MC55i
- Other modems may work using one of the implementations above. You can add your own implementation using the existing functionality as base.

## Other
- Debug functionality that lets you intercept incoming and outgoing data

## Additional Documentation
[Github Repository](https://github.com/hbjorgo/ATLib)
