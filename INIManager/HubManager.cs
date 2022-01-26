using System;
using System.Runtime.InteropServices;
using System.Text;

namespace INIManager
{
    public class HubManager
    {
        // Конструктор, принимающий путь к INI-файлу
        public HubManager(string Path)
        {
            path = Path;
        }

        // Конструктор без аргументов (путь к INI-файлу нужно будет задать отдельно)
        public HubManager() : this("") { }

        // Получить значение из INI-файла (по указанным секции и ключу) 
        /// <summary>
        /// Get value from selected section and key
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetString(string section, string key)
        {
            //Для получения значения
            StringBuilder buffer = new StringBuilder(SIZE);

            //Получить значение в buffer
            GetString(section, key, null, buffer, SIZE, path);

            //Вернуть полученное значение
            return buffer.ToString();
        }

        // Записать значение в INI-файл (по указанным секции и ключу)
        /// <summary>
        /// Write a new value in the selected key and section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void WriteString(string section, string key, string value)
        {
            //Записать значение в INI-файл
            WriteString(section, key, value, path);
        }

        // Получить ключ-значение в массиве string[] с "returned" именем из выбранной секции
        /// <summary>
        /// Get key-value in the string[] array with "returned" name from select section
        /// </summary>
        /// <param name="section">Section</param>
        /// <param name="returned"></param>
        /// <returns></returns>
        public bool GetSection(string section, out string[] returned)
        {
            returned = null;

            if (!System.IO.File.Exists(Path))
                return false;

            const uint MAX_BUFFER = 32767;

            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

            uint bytesReturned = Getsection(section, pReturnedString, MAX_BUFFER, path);

            if ((bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {
                Marshal.FreeCoTaskMem(pReturnedString);
                return false;
            }

            string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)(bytesReturned - 1));

            returned = returnedString.Split('\0');

            Marshal.FreeCoTaskMem(pReturnedString);
            return true;
        }

        // Удаляем выбранную секцию по ее имени
        /// <summary>
        /// Remove selected section from file
        /// </summary>
        /// <param name="section"></param>
        public void RemoveSection(string section)
        {
            WriteString(section, null, null);
        }

        // Удалить ключ из выбранной секции
        /// <summary>
        /// Delete key from selected section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        public void DeleteKey(string section, string key)
        {
            WriteString(section, key, null);
        }

        // Проверяем, есть ли такой ключ в выбранной секции
        /// <summary>
        /// Check if there is such a key in the selected section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyExists(string section, string key)
        {
            return GetString(section, key).Length > 0;
        }

        //Возвращает или устанавливает путь к INI файлу
        /// <summary>
        /// Set or get path of file
        /// </summary>
        public string Path { get { return path; } set { path = value; } }

        //Поля класса
        private const int SIZE = 1024; //Максимальный размер (для чтения значения из файла)
        private string path = null; //Для хранения пути к INI-файлу

        //Импорт функции GetPrivateProfileString (для чтения значений) из библиотеки kernel32.dll
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetString(string section, string key, string def, StringBuilder buffer, int size, string path);

        //Импорт функции WritePrivateProfileString (для записи значений) из библиотеки kernel32.dll
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern int WriteString(string section, string key, string str, string path);

        // Импорт функции GetPrivateProfileSection (чтение пар ключ-значение в секциях) из библиотеки kernel32.dll
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileSection")]
        private static extern uint Getsection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);
    }
}
