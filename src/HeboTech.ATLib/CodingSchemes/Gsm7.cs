using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.CodingSchemes
{
    /// <summary>
    /// Encode / decode GSM 7-bit strings (GSM 03.38 or 3GPP 23.038)
    /// </summary>
    internal static class Gsm7
    {
        // ` is not a conversion, just a untranslatable letter
        private static readonly Dictionary<Gsm7Extension, string> regularTable = new Dictionary<Gsm7Extension, string>()
        {
            { Gsm7Extension.Default, "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞ\x1bÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà" },
            { Gsm7Extension.Turkish, "@£$¥€éùıòÇLĞğCÅåΔ_ΦΓΛΩΠΨΣΘFEŞRßÉ !\"#¤%&'()ΞS,ş./0123456789*C<->?İABCDEFGHI:+L=NOPQRSTUVWXYJ;ÖMÜ§çabcdefghiZKlÑnopqrstuvwxyjÄömüà" },
            { Gsm7Extension.Spanish, "@£$¥èéùìòÇ`Øø`ÅåΔ_ΦΓΛΩΠΨΣΘΞ`ÆæßÉ !\"#¤%&'()*=,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ`¿abcdefghijklmnopqrstuvwxyzäöñüà" },
            { Gsm7Extension.Portugese, "@£$¥êéúíóçLÔôCÁáΔ_ªÇÀ∞^\\€ÓFEÂRÊÉ !\"#º%&'()|S,â./0123456789*C<->?ÍABCDEFGHI:+L=NOPQRSTUVWXYJ;ÕMÜ§~abcdefghiZKlÚnopqrstuvwxyjÃõmüà" },
            { Gsm7Extension.BengaliAndAssamese, "◌◌◌অআইঈউঊঋLঌ`C`এঁংঃওঔকখগঘঙFEছRঝঞঐ``ঠডঢণত)(চS,জ.ন !ট3456789থC`ধফ?012যর`ল```:দসপ◌ঽ◌ভম◌◌◌◌``◌শ;`হ়◌ব◌◌ুূৃৄghে◌ষl◌◌্ািীcdefwxiৈ`ডোৌo" },
            { Gsm7Extension.Gujarati, "◌◌◌અઆઇઈઉઊઋLઌઍC`એઁંઃઓઔકખગઘઙFEછRઝઞઐઑ`ઠડઢણત)(ચS,જ.ન !ટ3456789થC`ધફ?012યર`લળ`વ:દસપ◌ઽબભમ◌◌◌◌◌`◌શ;`હ઼◌◌◌◌ુૂૃૄૅhે◌ષl◌◌્ાિીcdefgxiૈ◌ૡોૌo" },
            { Gsm7Extension.Hindi, "◌◌◌अआइईउऊऋLऌऍCऎएँंःओऔकखगघङFEछRझञऐऑऒठडढणत)(चS,ज.न !ट3456789थCऩधफ?012यरऱलळऴव:दसप◌ऽबभम◌◌◌◌◌◌◌श;◌ह़◌◌◌◌ुूृॄॅॆे◌षॊ◌◌्ािीcdefghiै◌lोौo" },
            { Gsm7Extension.Kannada, "`ಂಃಅಆಇಈಉಊಋLಌ`Cಎಏಐ`ಒಓಔಕಖಗಘಙFEಛRಝಞ !ಟಠಪಢಣತ)(ಚS,ಜ.ನ0123456789ಥC`ಧಫ?ಬಭಮಯರಱಲಳ`ವ:ದಸಪ಼ಽಾಿೀುೂೃೄ`ೆೇಶ;ೊಹೌ್ೕabcdefghiೈಷlೋnopqrstuvwxyj`ೠmೢೣ" },
            { Gsm7Extension.Malayalam, "`ംഃഅആഇഈഉഊഋLഌ`Cഎഏഐ`ഒഓഔകഖഗഘങFEഛRഝഞ !ടഠഡഢണത)(ചS,ജ.ന0123456789ഥC`ധഫ?ബഭമയരറലളഴവ:ദസപ`ഽാിീുൂൃൄ`െേശ;ൊഹൌ്ൗabcdefghiൈഷlോnopqrstuvwxyj`ൡmൣ൹" },
            { Gsm7Extension.Oriya, "◌◌◌ଅଆଇଈଉଊଋLଌ`C`ଏଁଂଃଓଔକଖଗଘଙFEଛRଝଞଐ``ଠଡଢଣତ)(ଚS,ଜ.ନ !ଟ3456789ଥC`ଧଫ?012ଯର`ଲଳ`ଵ:ଦସପ◌ଽବଭମ◌◌◌ୄ``◌ଶ;`ହ଼◌◌◌◌ୁୂୃfghେ◌ଷl◌◌୍ାିୀcdevwxiୈ`ୠୋୌo" },
            { Gsm7Extension.Punjabi, "◌◌◌ਅਆਇਈਉਊ`L``C`ਏਁਂਃਓਔਕਖਗਘਙFEਛRਝਞਐ``ਠਡਢਣਤ)(ਚS,ਜ.ਨ !ਟ3456789ਥC`ਧਫ?012ਯਰ`ਲਲ`ਵ:ਦਸਪ◌`ਬਭਮ◌◌``਼`◌ਸ;`ਹ਼◌◌◌◌ੁੂef`hੇ਼`l◌◌੍ਾਿੀcduvgxi◌`◌ੋੌo" },
            { Gsm7Extension.Tamil, "`◌◌அஆஇஈஉஊ`L``Cஎஏஐஂஃஓஔக```ஙFE`R`ஞ `ஒ```ணத)(சS,ஜ.ந0!ட3456789`Cன``?`12யரறலளழவ:`ஸப``◌`ம◌◌```◌◌ஶ;◌ஹ◌◌ா◌◌ுூefgெே◌ஷொ◌ௌ்ௐிீcduvwhiை`lோno" },
            { Gsm7Extension.Telugu, "◌◌◌అఆఇఈఉఊఋLఌ`CఎఏఁంఃఓఔకఖగఘఙFEఛRఝఞఐ`ఒఠడఢణత)(చS,జ.న !ట3456789థC`ధఫ?012యరఱలళ`వ:దసప`ఽబభమ◌◌◌◌`◌◌శ;◌హ◌◌◌◌◌ుూృౄgెే◌షొ◌ౌ్ాిీcdefwhiై`lోno" },
            { Gsm7Extension.Urdu, "اآبٻڀپڦتۂٿLٹٽCٺټثجځڄڃڅچڇحخFEڌRډڊ !ڏڍذرڑړ)(دS,ڈ.ژ0123456789ڙCښږش?صضطظعفقکڪګ:زڱسمنںڻڼوۄەہھءیگ;◌ل◌◌◌abcdefghiېڳٍ◌ُٗٔqrstuvwxyjےlِno" },
        };
        private static readonly Dictionary<Gsm7Extension, string> extendedTable = new Dictionary<Gsm7Extension, string>()
        {
            { Gsm7Extension.Default, "````````````````````^```````````````````{}`````\\````````````[~]`|````````````````````````````````````€``````````````````````````" },
            { Gsm7Extension.Turkish, "````````````````````^```````````````````{}`````\\````````````[~]`|``````Ğ`İ`````````Ş```````````````ç`€`ğ`ı`````````ş````````````" },
            { Gsm7Extension.Spanish, "`````````ç``````````^```````````````````{}`````\\````````````[~]`|Á```````Í`````Ó`````Ú```````````á```€```í`````ó`````ú``````````" },
            { Gsm7Extension.Portugese, "`````ê```ç`Ôô`Áá``ΦΓ^ΩΠΨΣΘ`````Ê````````{}`````\\````````````[~]`|À```````Í`````Ó`````Ú`````ÃÕ````Â```€```í`````ó`````ú`````ãõ``â" },
            { Gsm7Extension.BengaliAndAssamese, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*০১`২৩৪৫৬৭৮৯যৠৡ◌{}◌৲৳৴৵\\৶৷৸৹়``ৢ``ৣ`[~]`|ABC৺EF`HI`KLMNOPQRSDUVGXYJ`````````T€`W``Z`````````````````````" },
            { Gsm7Extension.Gujarati, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*।॥`૦૧૨૩૪૫૬૭૮૯``{}`````\\````````````[~]`|ABCDEFGHIJKLMNOPQRSTUVWXYZ``````````€``````````````````````````" },
            { Gsm7Extension.Hindi, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*।॥`०१२३४५६७८९◌◌{}◌◌कखग\\जडढफयॠ॒॑◌॰़़़॓॔`़़़़़Eॡ◌ॣIॱ`[~]O|ABCDUFॢHYJKLMN`PQRST€VGX`Z````````````W````````" },
            { Gsm7Extension.Kannada, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*।॥`೦೧೨೩೪೫೬೭೮೯ೞೱ{}ೲ````\\````````````]~]`|ABCDEFGHIJKLMNOPQRSTUVWXYZ``````````€``````````````````````````" },
            { Gsm7Extension.Malayalam, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*।॥`൦൧൨൩൪൫൬൭൮൯൰൱{}൲൳൴൵ൺ\\ൻർൽൾൿ```````[~]`-ABCDEFGHIJKLMNOPQRSTUVWXYZ``````````€``````````````````````````" },
            { Gsm7Extension.Oriya, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*।॥`୦୧୨୩୪୫୬୭୮୯ଡଢ{}ୟ୰ୱ``\\``````଼଼````[~]`|ABCDE``HIJKLMNOPQRSTUFGXYZ``````````€VW````````````````````````" },
            { Gsm7Extension.Punjabi, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*।॥`੦੧੨੩੪੫੬੭੮੯ਖਗ{}ਜੜਫ◌`\\``````਼਼``਼`਼ੵ]`|ABCDE``HI`K[~NOPQRSTUFGXYJ`LM```````€VW``Z`````````````````````" },
            { Gsm7Extension.Tamil, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*।॥`௦௧௨௩௪௫௬௭௮௯௳௴{}௵௶௷௸௺\\````````````[~]`|ABCDEFGHIJKLMNOPQRSTUVWXYZ``````````€``````````````````````````" },
            { Gsm7Extension.Telugu, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*```౦౧౨౩౪౫౬౭౮౯ౘౙ{}౸౹౺౻౼\\౽౾౿`````````[~]`|ABCDEFGHIJKLMNOPQRSTUVWXYZ`````````````````````````````````````" },
            { Gsm7Extension.Urdu, "@£$¥¿\"¤%&'`*+`-/<=>¡^¡_#*؀؁`۰۱۲۳۴۵۶۷۸۹،؍{}؎؏◌◌◌\\◌◌؛؟ـ◌◌٫٬ٲٳۍؐؑؒ۔ؓؔBCDْ٘GHIJK[~]O|ARSTEFWXYZ`LMN`PQ```UV``````````````€``````````" },
        };

        public static bool IsGsm7Compatible(IEnumerable<char> text, Gsm7Extension lockingShift = Gsm7Extension.Default, Gsm7Extension singleShift = Gsm7Extension.Default)
        {
            string defaultString = regularTable[lockingShift];
            string extendedString = extendedTable[singleShift];

            for (int i = 0; i < text.Count(); i++)
            {
                char c = text.ElementAt(i);

                int intGSMTable = defaultString.IndexOf(c);
                if (intGSMTable != -1)
                    continue;

                int intExtendedTable = extendedString.IndexOf(c);
                if (intExtendedTable == -1)
                    return false;
            }

            return true;
        }

        public static byte[] EncodeToBytes(IEnumerable<char> text, Gsm7Extension lockingShift = Gsm7Extension.Default, Gsm7Extension singleShift = Gsm7Extension.Default)
        {
            string defaultString = regularTable[lockingShift];
            string extendedString = extendedTable[singleShift];

            List<byte> byteGSMOutput = new List<byte>();

            for (int i = 0; i < text.Count(); i++)
            {
                char c = text.ElementAt(i);

                int intGSMTable = defaultString.IndexOf(c);
                if (intGSMTable != -1)
                {
                    byteGSMOutput.Add((byte)intGSMTable);
                    continue;
                }

                int intExtendedTable = extendedString.IndexOf(c);
                if (intExtendedTable != -1)
                {
                    byteGSMOutput.Add(27);
                    byteGSMOutput.Add((byte)intExtendedTable);
                }
            }

            return byteGSMOutput.ToArray();
        }

        public static string DecodeFromBytes(IEnumerable<byte> bytes, Gsm7Extension lockingShift = Gsm7Extension.Default, Gsm7Extension singleShift = Gsm7Extension.Default)
        {
            string defaultString = regularTable[lockingShift];
            string extendedString = extendedTable[singleShift];

            StringBuilder sb = new StringBuilder(bytes.Count());

            bool isExtended = false;
            for (int i = 0; i < bytes.Count(); i++)
            {
                byte b = bytes.ElementAt(i);

                if (b == 27)
                {
                    if (i == bytes.Count() - 1) // If the ESC character is the last character for some reason - treat it as a space
                    {
                        sb.Append(' ');
                        continue;
                    }
                    isExtended = true;
                    continue;
                }

                if (isExtended)
                {
                    sb.Append(extendedString[b]);
                    isExtended = false;
                    continue;
                }

                if (b < defaultString.Length)
                {
                    sb.Append(defaultString[b]);
                    continue;
                }
            }

            return sb.ToString();
        }

        public static byte[] Pack(byte[] data, int paddingBits = 0)
        {
            // Array for all packed bits (n x 7)
            BitArray packedBits = new BitArray((int)Math.Ceiling(data.Length * 7 / 8.0) * 8 + paddingBits);

            // Loop through all characters
            for (int i = 0; i < data.Length; i++)
            {
                // Only 7 bits in each byte is data
                for (int j = 0; j < 7; j++)
                {
                    // For each 7 bits in each byte, add it to the bit array
                    int index = (i * 7) + j + paddingBits;
                    bool isSet = (data[i] & (1 << j)) != 0;
                    packedBits.Set(index, isSet);
                }
            }

            // Convert the bit array to a byte array
            byte[] packed = new byte[(int)Math.Ceiling(packedBits.Length / 8.0)];
            packedBits.CopyTo(packed, 0);

            // Return the septets packed as octets
            // If the last character is empty - skip it
            //if (packed[^1] == 0)
            //    return packed[..^1];
            return packed;
        }

        public static byte[] Unpack(byte[] data, int paddingBits = 0)
        {
            BitArray packedBits = new BitArray(data);
            packedBits.Length += paddingBits;
            packedBits.RightShift(paddingBits);
            byte[] unpacked = new byte[packedBits.Length / 7];

            byte value = 0;
            for (int i = 0; i < unpacked.Length * 7; i += 7)
            {
                for (int j = 0; j < 7; j++)
                {
                    value |= packedBits[i + j] ? (byte)(1 << j) : (byte)(0 << j);
                }
                unpacked[i / 7] = value;
                value = 0;
            }

            // If the last character is empty - skip it.
            // It means that one bit of the last octet was used by the last character and the last 7 bits weren't used
            if (unpacked[^1] == 0)
                return unpacked[..^1];
            return unpacked;
        }
    }
}
