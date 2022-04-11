using System.IO;

/*
 * утилитарный логгер для поиска проблем в синхронизации
 */
namespace Game.Fabros.Net.ClientServer
{
    public class SyncLog
    {
        private StreamWriter file;

        public SyncLog(string name)
        {
            try
            {
                file = new StreamWriter(name);
                file.AutoFlush = true;
            }
            catch (IOException e)
            {
            }
        }

        public void WriteLine(string str)
        {
            file?.Write(str + "\n");
        }

        public void Close()
        {
            file?.Close();
            file = null;
        }
    }
}