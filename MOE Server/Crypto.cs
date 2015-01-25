using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MOE_Server
{
    class Crypto
    {
        public string Crypt(string plainText, string cryptoKey)
        {
            string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ <>:";
            int lenght = alphabet.Length;
            char[,] vijenerTable = new char[lenght, lenght];
            int a = 0;
            //sglobqvane na tablicata na Vijener
            for (int i = 0; i < lenght; i++)
            {
                for (int j = 0; j < lenght; j++)
                {
                    a = j + i;
                    if (a >= lenght)
                    {
                        a = a % lenght;
                    }
                    vijenerTable[i, j] = alphabet[a];
                }
            }

            string key = cryptoKey;
            string key_on_s = "";
            bool flag;
            int x = 0, y = 0;
            char duplicate;
            string result = "";
            string text = plainText;


            for (int i = 0; i < text.Length; i++)
            {
                key_on_s += key[i % key.Length];
            }

            for (int i = 0; i < text.Length; i++)
            {
                //tyrsene na simvola v tablicata
                int l = 0;
                flag = false;
                //dokato ne se nameri simvola
                while ((l < lenght) && (flag == false))
                {
                    //ako simvola e nameren
                    if (key_on_s[i] == vijenerTable[l, 0])
                    {
                        x = l;
                        flag = true;
                    }
                    l++;
                }

                duplicate = text[i];

                l = 0;
                flag = false;

                //tyrsim v vtoriq stylb dokato ne se nameri simvola
                while ((l < lenght) && (flag == false))
                {
                    //ako simvola e nameren
                    if (duplicate == vijenerTable[0, l])
                    {
                        y = l;
                        flag = true;
                    }
                    l++;
                }
                //dobavqme simvola kym krainiq rezultat

                result += vijenerTable[x, y];

            }

            return result;

        }

        public string DeCrypt(string cryptedText, string cryptoKey)
        {
            string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ <>:";
            int lenght = alphabet.Length;
            char[,] vijenerTable = new char[lenght, lenght];
            int a = 0;
            //sglobqvane na tablicata na Vijener
            for (int i = 0; i < lenght; i++)
            {
                for (int j = 0; j < lenght; j++)
                {
                    a = j + i;
                    if (a >= lenght)
                    {
                        a = a % lenght;
                    }
                    vijenerTable[i, j] = alphabet[a];
                }
            }

            string key = cryptoKey;
            string key_on_s = "";
            bool flag;
            int x = 0, y = 0;
            char duplicate;
            string result = "";
            string text = cryptedText;

            for (int i = 0; i < text.Length; i++)
            {
                key_on_s += key[i % key.Length];
            }

            for (int i = 0; i < text.Length; i++)
            {
                //tyrsene na simvola v tablicata
                int l = 0;
                flag = false;
                //dokato ne se nameri simvola
                while ((l < lenght) && (flag == false))
                {
                    //ako simvola e nameren
                    if (key_on_s[i] == vijenerTable[l, 0])
                    {
                        x = l;
                        flag = true;
                    }
                    l++;
                }

                duplicate = text[i];
                l = 0;
                flag = false;
                //tyrsim v vtoriq stylb dokato ne se nameri simvola
                while ((l < lenght) && (flag == false))
                {
                    //ako simvola e nameren
                    if (duplicate == vijenerTable[x, l])
                    {
                        y = l;
                        flag = true;
                    }
                    l++;
                }
                //dobavqme simvola kym krainiq rezultat

                result += vijenerTable[0, y];

            }

            return result;
        }
    }
}
