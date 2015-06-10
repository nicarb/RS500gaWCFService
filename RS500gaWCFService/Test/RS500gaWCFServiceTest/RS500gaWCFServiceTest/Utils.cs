using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS500gaWCFServiceTest
{
    class Utils
    {
        public const string TRANSLATE_RU_W1 = "Сатерн";
        public const string TRANSLATE_EN_W1 = "Southern";
        public const string TRANSLATE_RU_W2 = "Фанк";
        public const string TRANSLATE_EN_W2 = "Funk";
        public const string TRANSLATE_RU_W3 = "Соул";
        public const string TRANSLATE_EN_W3 = "Soul";
        public const string TRANSLATE_RU_W4 = "Поп";
        public const string TRANSLATE_EN_W4 = "Pop";
        public const string TRANSLATE_RU_W5 = "Блюз";
        public const string TRANSLATE_EN_W5 = "Blues";
        public const string TRANSLATE_RU_W6 = "Рок";
        public const string TRANSLATE_EN_W6 = "Rock";
        public const string TRANSLATE_RU_W7 = "Арт";
        public const string TRANSLATE_EN_W7 = "Art";
        public const string TRANSLATE_RU_W8 = "Электронная";
        public const string TRANSLATE_EN_W8 = "Electronic";
        public const string TRANSLATE_RU_W9 = "Кантри";
        public const string TRANSLATE_EN_W9 = "Country";
        public const string TRANSLATE_RU_W10 = "Фолк";
        public const string TRANSLATE_EN_W10 = "Folk";
        public const string TRANSLATE_RU_W11 = "Мировая";
        public const string TRANSLATE_EN_W11 = "World";
        public const string TRANSLATE_RU_W12 = "Хип-хоп";
        public const string TRANSLATE_EN_W12 = "Hip-hop";
        public const string TRANSLATE_RU_W13 = "Рэп";
        public const string TRANSLATE_EN_W13 = "Rap";
        public const string TRANSLATE_RU_W14 = "Инди";
        public const string TRANSLATE_EN_W14 = "Indie";
        public const string TRANSLATE_RU_W15 = "Гангста";
        public const string TRANSLATE_EN_W15 = "Gangsta";
        public const string TRANSLATE_RU_W16 = "Психоделик";
        public const string TRANSLATE_EN_W16 = "Psychedelic";
        public const string TRANSLATE_RU_W17 = "Джаз";
        public const string TRANSLATE_EN_W17 = "Jazz";
        public const string TRANSLATE_RU_W18 = "Модальный";
        public const string TRANSLATE_EN_W18 = "Modal";
        //public const string TRANSLATE_RU_W19 = "";
        //public const string TRANSLATE_EN_W19 = "";
        //public const string TRANSLATE_RU_W20 = "";
        //public const string TRANSLATE_EN_W20 = "";
        //public const string TRANSLATE_RU_W21 = "";
        //public const string TRANSLATE_EN_W21 = "";
        //public const string TRANSLATE_RU_W22 = "";
        //public const string TRANSLATE_EN_W22 = "";
        //public const string TRANSLATE_RU_W23 = "";
        //public const string TRANSLATE_EN_W23 = "";


        public static Dictionary<string, string> TRANSLATE_RU_EN
        {
            get
            {
                Dictionary<string, string> translate_ru_en = new Dictionary<string, string>();
                translate_ru_en.Add(TRANSLATE_RU_W1, TRANSLATE_EN_W1);
                translate_ru_en.Add(TRANSLATE_RU_W2, TRANSLATE_EN_W2);
                translate_ru_en.Add(TRANSLATE_RU_W3, TRANSLATE_EN_W3);
                translate_ru_en.Add(TRANSLATE_RU_W4, TRANSLATE_EN_W4);
                translate_ru_en.Add(TRANSLATE_RU_W5, TRANSLATE_EN_W5);
                translate_ru_en.Add(TRANSLATE_RU_W6, TRANSLATE_EN_W6);
                translate_ru_en.Add(TRANSLATE_RU_W7, TRANSLATE_EN_W7);
                translate_ru_en.Add(TRANSLATE_RU_W8, TRANSLATE_EN_W8);
                translate_ru_en.Add(TRANSLATE_RU_W9, TRANSLATE_EN_W9);
                translate_ru_en.Add(TRANSLATE_RU_W10, TRANSLATE_EN_W10);
                translate_ru_en.Add(TRANSLATE_RU_W11, TRANSLATE_EN_W11);
                translate_ru_en.Add(TRANSLATE_RU_W12, TRANSLATE_EN_W12);
                translate_ru_en.Add(TRANSLATE_RU_W13, TRANSLATE_EN_W13);
                translate_ru_en.Add(TRANSLATE_RU_W14, TRANSLATE_EN_W14);
                translate_ru_en.Add(TRANSLATE_RU_W15, TRANSLATE_EN_W15);
                translate_ru_en.Add(TRANSLATE_RU_W16, TRANSLATE_EN_W16);
                translate_ru_en.Add(TRANSLATE_RU_W17, TRANSLATE_EN_W17);
                translate_ru_en.Add(TRANSLATE_RU_W18, TRANSLATE_EN_W18);
                //translate_ru_en.Add(TRANSLATE_RU_W19, TRANSLATE_EN_W19);
                //translate_ru_en.Add(TRANSLATE_RU_W20, TRANSLATE_EN_W20);
                //translate_ru_en.Add(TRANSLATE_RU_W21, TRANSLATE_EN_W21);
                //translate_ru_en.Add(TRANSLATE_RU_W22, TRANSLATE_EN_W22);
                //translate_ru_en.Add(TRANSLATE_RU_W23, TRANSLATE_EN_W23);
                return translate_ru_en;
            }
        }

    }
}
