using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace TripleDES
{
    public class TripleDES
    {
        private TripleDESCryptoServiceProvider DES;
        private byte[] key;
        private byte[] iv;

        //These properties get and set values for the Key and IV
        public byte Key
        {
            set{key = value;}

            get{return key;}
        }

        public byte IV
        {
            set { iv = value; }

            get { return iv; }
        }

        //This function creates a 192 bit key and a 64 bit IV based on two MD5 Methods
        public string CreateKeys
        {

        }//end function CreateKeys
    }//end class TripleDES
}//end Namespace TripleDES
