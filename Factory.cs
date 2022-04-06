using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Lab8_OOP
{
    interface IStorage
    {
        public void saveObjects();
        public void loadObjects();
    }
    class MyStorage: Storage, IStorage
    {
        public virtual CObject createObj(char code)
        {
            return null;
        }
        public void saveObjects()
        {
            string way = @"save.txt";
            StreamWriter file = new StreamWriter(way, false);
            // сохраняем инф. о кол-ве эл-в
            file.WriteLine(k);
            // сохраняем инф. обо всех объектах, находящихся в хранилище
            for (int i=0; i<k; ++i)
            {
                st[i].save(file);
            }
            file.Close();
        }
        public void loadObjects()
        {
            string way = @"save.txt";
            StreamReader file = new StreamReader(way);
            // считываем кол-во эл-в в хран.
            int count = Int32.Parse(file.ReadLine());
            k = 0;
            // поочередно считываем данные обо всех сохр. объектах
            char code;
            for (int i = 0; i < count; ++i)
            {
                code = Convert.ToChar(file.ReadLine());
                CObject new_obj = createObj(code);
                new_obj.load(file);
                add(new_obj);

            }
            file.Close();
        }
    }
    class Factory: MyStorage
    {
        public override CObject createObj(char code)
        {
            switch (code) {
                case 'C': return new CCircle(0, 0, Form1.c);
                case 'T': return new CTriangle(0, 0, Form1.c);
                case 'R': return new CRectangle(0, 0, Form1.c);
                case 'S': return new CSquare(0, 0, Form1.c);
                case 'E': return new CEllipse(0, 0, Form1.c);
                case 'r': return new CRhomb(0, 0, Form1.c);
                case 't': return new CTrapeze(0, 0, Form1.c);
                case 'P': return new CPolygon(0, 0, Form1.c);
                case 'L': return new CLine(default, 0, 0, Form1.c);
                case 'G': return new CGroup();
                default: return null;
            }
        }
    }
}
