using System.IO;

   public class MyStream : StreamReader
    {
        private uint _pos;
        public MyStream(string path,System.Text.Encoding enc) : base(path,enc)
        {

        }
        public override string ReadLine()
        {
            char current;
            int i;
            string line = null;
            while((i=base.Read())!=-1)
            {
                current = (char)i;
                _pos++;
                if (IsFeed(current))
                {
                    if ((i = base.Peek()) != -1)
                    {
                        if (!IsFeed((char)i))
                            break; //avoid.

                    }
                }
                else line += current;
            }
            return line;
        }
        private bool IsFeed(char c)
        {
            if(c=='\r'||c=='\n')
            {
                return true;
            }
            return false;
        }
        public uint Seek(uint pos)
        {
        this.DiscardBufferedData();
            this.BaseStream.Seek(pos, SeekOrigin.Begin);
            _pos = pos;
            return pos;
        }
        public uint Pos
        {
            get
            {
                return _pos;
            }
        }
    } 
