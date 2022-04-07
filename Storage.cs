using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lab8_OOP
{
    public class Storage
    {
        protected int n, k; // размер и кол-во эл-в
        public CObject[] st;
        public System.EventHandler observers;
        public Storage()
        {
            n = 1;
            st = new CObject[n];
            st[0] = null; // или default
            k = 0;
        }
        public Storage(int size)
        {
            n = size;
            st = new CObject[n];
            k = 0;
            for (int i = 0; i < n; ++i)
                st[i] = null;
        }
        public void add(CObject new_el)
        {
            if (k < n)
            {
                st[k] = new_el;
                k = k + 1;
            }
            else
            {
                n = n * 2;
                CObject[] st_ = new CObject[n];
                for (int i = 0; i < k; ++i)
                    st_[i] = st[i];
                st_[k] = new_el;
                k = k + 1;
                for (int i = k; i < n; ++i)
                    st_[i] = null;
                st = st_;
            }
            notify_observers();
        }
        public void del(int ind)
        {
            for (int i = ind; i < k - 1; ++i)
                st[i] = st[i + 1];
            k = k - 1;
            st[k] = null;
            notify_observers();
        }
        public CObject get_el(int ind)
        {
            return st[ind];
        }
        public int get_count()
        {
            return k;
        }
        // методы для прохождения по всем элементам хранилища и выполнения каких-то действий с ними
        public void del_highlighted_objects()
        {
            for (int i = 0; i < k; ++i)
                if (st[i].get_highlighted() == true)
                {
                    del(i);
                    i = i - 1;
                }
        }
        public void del_all_objects()
        {
            for (int i = 0; i < k; ++i)
            {
                del(i);
                i = i - 1;
            }
        }
        public void draw_objects(PaintEventArgs e)
        {
            for (int i = 0; i < k; ++i)
                st[i].draw(e);
            notify_observers();
        }
        public void resize_highlighted_objects(bool size_delt, int pbW, int pbH, int d)
        {
            for (int i = 0; i < k; ++i)
                if (st[i].get_highlighted() == true)
                    st[i].resize(size_delt, pbW, pbH, d);
        }
        public void move_highlighted_objects(int move, int pbW, int pbH, int d)
        {
            for (int i = 0; i < k; i++)
                if (st[i].get_highlighted() == true)
                    st[i].move(move, pbW, pbH, d);
        }
        

        public void setColor_highlighted_objects(Color new_color)
        {
            for (int i = 0; i < k; ++i)
                if (st[i].get_highlighted() == true)
                    st[i].set_color(new_color);
        }
        public void setAllHighlightFalse() // устанавливаем свойство выделенности для всех объектов - false
        {
            for (int i = 0; i < k; ++i)
                if (st[i].get_highlighted() == true)
                    st[i].change_highlight();
            notify_observers();
        }
        public int mouseClick_on_Object(int x, int y) // возвращает индекс объекта, на который попала мышь или - число -1
        {
            int index = -1;
            for (int i = 0; i < k; ++i)
                if (st[i].mouseClick_on_Object(x, y) == true)
                    index = i;
            return index;
        }
        public void change_highlight_of_object(int ind)
        {
            if (ind != -1)
                st[ind].change_highlight();
            notify_observers();
        }

        // методы для работы с группами 
        public void add_new_group()
        {
            // добавляем все выделенные объекты в новую группу,
            // удаляем их из хранилища, группу добавляем в хранилище
            CGroup new_group = new CGroup();
            int i = 0;
            while (i < k)
            {
                if (st[i].get_highlighted() == true)
                {
                    st[i].change_highlight();
                    new_group.addObject(st[i]);
                    del(i);
                }
                else i++;
            }
            if (new_group.get_count() != 0)
            {
                add(new_group);
                // объекты внутри группы и группа(рамка) выделяются
                st[i].change_highlight();
            }
            notify_observers();
        }
        public void del_highlighted_groups()
        {
            // из всех выделенных групп переносим объекты в хранилище и удаляем группы из хранилища
            // цикл проходит задом наперед на случай, если группа содержала группу, а мы не хотим 
            // разгруппировывать внутр. группу
            for (int i = k - 1; i >= 0; --i)
                if (st[i].get_highlighted() == true && st[i].classname() == "CGroup")
                {
                    for (int j = 0; j < (st[i] as CGroup).get_count(); ++j)
                        add((st[i] as CGroup).get_el(j));
                    del(i);
                    i++;
                }
            notify_observers();
        }
        public void notify_observers()
        {
            observers.Invoke(this, null);
        }

        // методы свойства sticky
        public void set_sticky_of_objects(bool state) // задаем свойсво липкости выделенным объектам по флагу state 
        {
            for (int i = 0; i < k; ++i)
                if (st[i].get_highlighted() == true)
                {
                    if (state == true)

                        st[i].set_stickiness();
                    else st[i].set_nonStickiness();
                }
        }
        public void check_new_objects_sticked(PaintEventArgs e) // обновляем наблюдателей каждого липкого объекта
        {
            Region rgn;
            for (int i = 0; i < k; ++i)
            {
                if (st[i].get_sticky() == true)
                {
                    // определяем область липкого объекта
                    rgn = st[i].get_region();
                    // для каждого объекта из хранилища, проверяем пересечение его области с областью липкого объекта
                    for (int j = 0; j < k; ++j)
                    {
                        if (i != j)
                        {   // в rgn записываем Region пересечения объектов
                            rgn.Intersect(st[j].get_region());

                            // если пересечение не пусто, то добавляем нового наблюдателя,
                            // иначе - удаляем
                            if (!rgn.IsEmpty(e.Graphics))
                            {
                                st[i].add_stickyObserver(st[j]);
                                e.Graphics.FillRegion(Brush.stickyBrush, rgn);
                            }
                            else st[i].del_stickyObserver(st[j]);

                            // возвращаем rgn область липкого объекта вместо области пересечения
                            rgn = st[i].get_region();
                        }
                    }
                }

            }
        }
    };
}
