using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Boxes.Apple
{
    /**
 * Created by sannies on 10/12/13.
 */
    public abstract class AppleDataBox : AbstractBox
    {
        private static Dictionary<string, string> language = new Dictionary<string, string>();

        static AppleDataBox()
        {
            language.Add("0", "English");
            language.Add("1", "French");
            language.Add("2", "German");
            language.Add("3", "Italian");
            language.Add("4", "Dutch");
            language.Add("5", "Swedish");
            language.Add("6", "Spanish");
            language.Add("7", "Danish");
            language.Add("8", "Portuguese");
            language.Add("9", "Norwegian");
            language.Add("10", "Hebrew");
            language.Add("11", "Japanese");
            language.Add("12", "Arabic");
            language.Add("13", "Finnish");
            language.Add("14", "Greek");
            language.Add("15", "Icelandic");
            language.Add("16", "Maltese");
            language.Add("17", "Turkish");
            language.Add("18", "Croatian");
            language.Add("19", "Traditional_Chinese");
            language.Add("20", "Urdu");
            language.Add("21", "Hindi");
            language.Add("22", "Thai");
            language.Add("23", "Korean");
            language.Add("24", "Lithuanian");
            language.Add("25", "Polish");
            language.Add("26", "Hungarian");
            language.Add("27", "Estonian");
            language.Add("28", "Lettish");
            language.Add("29", "Sami");
            language.Add("30", "Faroese");
            language.Add("31", "Farsi");
            language.Add("32", "Russian");
            language.Add("33", "Simplified_Chinese");
            language.Add("34", "Flemish");
            language.Add("35", "Irish");
            language.Add("36", "Albanian");
            language.Add("37", "Romanian");
            language.Add("38", "Czech");
            language.Add("39", "Slovak");
            language.Add("40", "Slovenian");
            language.Add("41", "Yiddish");
            language.Add("42", "Serbian");
            language.Add("43", "Macedonian");
            language.Add("44", "Bulgarian");
            language.Add("45", "Ukrainian");
            language.Add("46", "Belarusian");
            language.Add("47", "Uzbek");
            language.Add("48", "Kazakh");
            language.Add("49", "Azerbaijani");
            language.Add("50", "AzerbaijanAr");
            language.Add("51", "Armenian");
            language.Add("52", "Georgian");
            language.Add("53", "Moldavian");
            language.Add("54", "Kirghiz");
            language.Add("55", "Tajiki");
            language.Add("56", "Turkmen");
            language.Add("57", "Mongolian");
            language.Add("58", "MongolianCyr");
            language.Add("59", "Pashto");
            language.Add("60", "Kurdish");
            language.Add("61", "Kashmiri");
            language.Add("62", "Sindhi");
            language.Add("63", "Tibetan");
            language.Add("64", "Nepali");
            language.Add("65", "Sanskrit");
            language.Add("66", "Marathi");
            language.Add("67", "Bengali");
            language.Add("68", "Assamese");
            language.Add("69", "Gujarati");
            language.Add("70", "Punjabi");
            language.Add("71", "Oriya");
            language.Add("72", "Malayalam");
            language.Add("73", "Kannada");
            language.Add("74", "Tamil");
            language.Add("75", "Telugu");
            language.Add("76", "Sinhala");
            language.Add("77", "Burmese");
            language.Add("78", "Khmer");
            language.Add("79", "Lao");
            language.Add("80", "Vietnamese");
            language.Add("81", "Indonesian");
            language.Add("82", "Tagalog");
            language.Add("83", "MalayRoman");
            language.Add("84", "MalayArabic");
            language.Add("85", "Amharic");
            language.Add("87", "Galla");
            language.Add("87", "Oromo");
            language.Add("88", "Somali");
            language.Add("89", "Swahili");
            language.Add("90", "Kinyarwanda");
            language.Add("91", "Rundi");
            language.Add("92", "Nyanja");
            language.Add("93", "Malagasy");
            language.Add("94", "Esperanto");
            language.Add("128", "Welsh");
            language.Add("129", "Basque");
            language.Add("130", "Catalan");
            language.Add("131", "Latin");
            language.Add("132", "Quechua");
            language.Add("133", "Guarani");
            language.Add("134", "Aymara");
            language.Add("135", "Tatar");
            language.Add("136", "Uighur");
            language.Add("137", "Dzongkha");
            language.Add("138", "JavaneseRom");
            language.Add("32767", "Unspecified");
        }

        int dataType;
        int dataCountry;
        int dataLanguage;

        protected AppleDataBox(String type, int dataType) : base(type)
        {
            this.dataType = dataType;
        }

        public String getLanguageString()
        {
            String lang = language["" + dataLanguage];
            if (lang == null)
            {
                ByteBuffer b = ByteBuffer.wrap(new byte[2]);
                IsoTypeWriter.writeUInt16(b, dataLanguage);
                b.reset();
                return new Locale(IsoTypeReader.readIso639(b)).getDisplayLanguage();
            }
            else
            {
                return lang;
            }
        }

        protected override long getContentSize()
        {
            return getDataLength() + 16;
            // actualdatalength + dataheader
        }

        public int getDataType()
        {
            return dataType;
        }

        public int getDataCountry()
        {
            return dataCountry;
        }

        public void setDataCountry(int dataCountry)
        {
            this.dataCountry = dataCountry;
        }

        public int getDataLanguage()
        {
            return dataLanguage;
        }

        public void setDataLanguage(int dataLanguage)
        {
            this.dataLanguage = dataLanguage;
        }

        protected ByteBuffer parseDataLength4ccTypeCountryLanguageAndReturnRest(ByteBuffer content)
        {
            int dataLength = content.getInt();
            int data4cc = content.getInt(); // 'data'
            dataType = content.getInt();
            dataCountry = content.getShort();
            if (dataCountry < 0)
            {
                dataCountry += (1 << 16);
            }
            dataLanguage = content.getShort();
            if (dataLanguage < 0)
            {
                dataLanguage += (1 << 16);
            }
            ByteBuffer data = (ByteBuffer)content.duplicate().slice().limit(dataLength - 16);
            ((Buffer)content).position(dataLength - 16 + content.position());
            return data;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            ByteBuffer data = parseDataLength4ccTypeCountryLanguageAndReturnRest(content);
            parseData(data);
        }

        protected void getContent(ByteBuffer byteBuffer)
        {
            writeDataLength4ccTypeCountryLanguage(byteBuffer);
            byteBuffer.put(writeData());
        }

        protected abstract byte[] writeData();

        protected abstract void parseData(ByteBuffer data);

        protected abstract int getDataLength();

        protected void writeDataLength4ccTypeCountryLanguage(ByteBuffer content)
        {
            content.putInt(getDataLength() + 16);
            content.put("data".getBytes());
            content.putInt(dataType);
            IsoTypeWriter.writeUInt16(content, dataCountry);
            IsoTypeWriter.writeUInt16(content, dataLanguage);
        }
    }
}
