using System;

using Rock;

namespace com.blueboxmoon.AcmeCertificate
{
    [Serializable]
    public class BindingData
    {
        public string Site { get; set; }

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public string Domain { get; set; }

        public BindingData()
        {
        }

        public BindingData( string text )
        {
            var elements = text.Split( new char[] { ',' } );

            if ( elements.Length == 4 )
            {
                Site = elements[0];
                IPAddress = elements[1];
                Port = elements[2].AsInteger();
                Domain = elements[3];
            }
        }

        public override string ToString()
        {
            return string.Format( "{0},{1},{2},{3}", Site, IPAddress, Port, Domain );
        }
    }
}
