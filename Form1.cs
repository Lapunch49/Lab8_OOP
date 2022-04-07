using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Drawing.Drawing2D;

namespace Lab8_OOP
{
    public partial class Form1 : Form
    {
        public bool ctrlPress = false; // для выделения нескольких объектов
        public bool shiftPress = false; // для увеличения размера (shift +)
        public static Color c = Color.LightPink;
        Factory storObj = new Factory();
        public static CObject[] ObjList =
            {new CCircle(0,0,c), //0
            new CTriangle(0,0,c), //1
            new CRectangle(0,0, c), //2
            new CSquare(0,0, c), //3
            new CEllipse(0,0,c), //4
            new CRhomb(0,0,c), //5
            new CTrapeze(0,0,c), //6
            new CPolygon(0,0,c) //7
        };
        string cur_select = "CCircle"; // текущий выбор фигуры, которая будет создаваться при нажатии на пустое место 
        CObject line_st = null; // точка - начало отрезка
        public int mouseX = 0;
        public int mouseY = 0;
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            storObj.observers += new EventHandler(this.UpdateTreeView);
            // меняю оформление концов pen для рисовании отрезков
            Brush.normPen.EndCap = LineCap.RoundAnchor;
            Brush.normPen.StartCap = LineCap.RoundAnchor;
            Brush.highlightPen.EndCap = LineCap.RoundAnchor;
            Brush.highlightPen.StartCap = LineCap.RoundAnchor;
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // удаление объектов
            if (e.KeyCode == Keys.Delete) // выделенные объекты удалятся из хранилища, и произойдет перерисовка
            {
                storObj.del_highlighted_objects();
            }


            // увеличение-уменьшение размера объектов 
            bool changeSize = false;
            bool size_delt = false;

            // увеличение размера объектов 
            if (e.KeyCode == Keys.Oemplus && shiftPress == true)
            {
                changeSize = true;
                size_delt = true;
            }
            // уменьшение размера объектов
            if (e.KeyCode == Keys.OemMinus)
                changeSize = true;

            // применяем изменения к объектам
            if (changeSize == true)
                storObj.resize_highlighted_objects(size_delt, pictureBox1.Width, pictureBox1.Height, 10);



            // передвижение объектов вправо-влево-вверх-вниз
            int move = 0;
            switch (e.KeyCode)
            {
                case Keys.Left: { move = -1; break; }
                case Keys.Right: { move = 1; break; }
                case Keys.Up: { move = 2; break; }
                case Keys.Down: { move = -2; break; }
                default: break;
            }
            if (move != 0)
            {
                storObj.move_highlighted_objects(move, pictureBox1.Width, pictureBox1.Height, 10);
            }

            pictureBox1.Invalidate();

            ctrlPress = e.Control;
            shiftPress = e.Shift;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            ctrlPress = e.Control;
            shiftPress = e.Shift;
        }
        private void setAllHighlightFalse()
        {
            // в хранилище меняем у выделенных объектов св. выделенности
            for (int i = 0; i < storObj.get_count(); ++i)
                if (storObj.get_el(i).get_highlighted() == true)
                    storObj.get_el(i).change_highlight();
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int ind = -1; // попадание по объекту с индексом ind

                // определяем попадание по существующему объекту 
                ind = storObj.mouseClick_on_Object(e.X, e.Y);

                // не попали по объекту 
                if (ind == -1)
                {
                    // убираем все выделения
                    storObj.setAllHighlightFalse();

                    // создаем новый объект
                    // если нов. об. не линия
                    if (cur_select != "CLine")
                        {
                        CObject newObj = createObj();
                        newObj = newObj.new_obj(e.X, e.Y, Brush.normBrush.Color);
                        storObj.add(newObj);

                        //считаем, что мы попали по нему
                        ind = storObj.get_count() - 1;
                    }
                    else if (cur_select == "CLine")
                        if (line_st == null)
                        {
                            line_st = new CObject(e.X, e.Y, Brush.normPen.Color);
                        }
                        else
                        {
                            CObject newObj = new CLine(line_st, e.X, e.Y, Brush.normPen.Color);
                            line_st = null;
                            storObj.add(newObj);
                            ind = storObj.get_count() - 1;
                        }
                }
                else
                {
                    // попали по сущ-му объекту
                    // дорисовываем отрезок, если 1 точка отрезка уже есть 
                    if (cur_select == "CLine" && line_st != null)
                    {
                        CObject newObj = new CLine(line_st, e.X, e.Y, Brush.normPen.Color);
                        line_st = null;
                        storObj.add(newObj);
                        ind = storObj.get_count() - 1;
                    }
                    // если не дорисовываем отрезок, проверяем ctrl
                    // если ctrl не зажат - убираем остальные выделения
                    else if (ctrlPress != true)
                    {
                        storObj.setAllHighlightFalse();
                    }
                }

                // выделяем объект, по которому попали
                storObj.change_highlight_of_object(ind);

                pictureBox1.Invalidate();
            }
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < storObj.get_count(); ++i)
                if (storObj.get_el(i) != null)
                    storObj.get_el(i).draw(e);
            // возвращаем цвет кистям
            Brush.normBrush.Color = Brush.Color;
            Brush.normPen.Color = Brush.Color;
            // рисуем начало отрезка, если оно есть 
            if (line_st != null)
                e.Graphics.DrawLine(Brush.normPen, line_st.get_x(), line_st.get_y(), mouseX, mouseY);
            //line_st.draw(e);
            storObj.check_new_objects_sticked(e); 
        }
        private CObject createObj()
        {
            for (int i = 0; i < ObjList.Length; ++i)
            {
                if (ObjList[i].classname() == cur_select)
                    return ObjList[i];
            }
            return new CCircle(0, 0, c);
        }
        private void btn_color_Click(object sender, EventArgs e)
        {
            Color new_color = ((Button)sender).BackColor;
            // у выделенных объектов меняем цвет
            storObj.setColor_highlighted_objects(new_color);
            // меняем текущий цвет, используемый при рисовании новых фигур
            Brush.Color = new_color;
            Brush.normBrush.Color = new_color;
            Brush.normPen.Color = new_color;
            //Brush.stickyBrush.Dispose();
            //Brush.stickyBrush = new HatchBrush(HatchStyle.LargeConfetti, Color.Black, new_color);
            // для отрезка
            if (line_st != null)
                line_st.set_color(new_color);
        }

        private void btn_other_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                btn_other.BackColor = colorDialog1.Color;
                btn_color_Click(btn_other, e);
            }
        }
        private void btn_shape_Click(object sender, EventArgs e)
        {
            cur_select = ((Button)sender).Name.ToString();
            line_st = null;
        }
        private void btn_clear_Click(object sender, EventArgs e)
        {
            storObj.del_all_objects();
            pictureBox1.Invalidate();
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
            if (cur_select == "CLine" && line_st != null)
                pictureBox1.Invalidate();
        }
        private void btn_Group_Click(object sender, EventArgs e)
        {   // добавляем все выделенные объекты в новую группу,
            // удаляем их из хранилища, группу добавляем в хранилище
            storObj.add_new_group();
            pictureBox1.Invalidate();
        }
        private void btn_DisGroup_Click(object sender, EventArgs e)
        {   
            // из всех выделенных групп переносим объекты в хранилище и удаляем группы из хранилища
            storObj.del_highlighted_groups();
            pictureBox1.Invalidate();
        }
        private void btn_save_Click(object sender, EventArgs e)
        {
            storObj.saveObjects();
        }
        private void btn_load_Click(object sender, EventArgs e)
        {
            btn_clear_Click(sender, e);
            storObj.loadObjects();
            pictureBox1.Invalidate();
        }

        // для TreeView
        private void processNode(TreeNode tn, CObject obj)
        {
            // Создаем у узла дерева tn новый дочерний узел t;
            TreeNode t = new TreeNode(obj.classname());
            tn.Nodes.Add(t);
            // если объект выделенный, выделяяем его и в treeView
            if (obj.get_highlighted() == true)
                t.BackColor = Color.LightCoral;

            // если объект obj является группой, то:
            if (t.Text == "CGroup")
            {   // для всех объектов oo из группы obj создаем узлы
                CGroup group = obj as CGroup;
                if (group != null) {
                    for (int i = 0; i < group.get_count(); ++i)
                    {
                        processNode(t, group.get_el(i));
                    }
                    //t.Expand();
                }
            }
        }
        public void UpdateTreeView(object sender, EventArgs e) // обновляет TreeView в соответсвии с состоянием хранилища
        {
            treeView_stor.Nodes.Clear();
            treeView_stor.Nodes.Add("Storage");
            int j = 0;
            if (storObj.get_count()!=0)
                processNode(treeView_stor.Nodes[0], storObj.get_el(j));
            j++;
            treeView_stor.Nodes[0].Expand();
            while (j < storObj.get_count())
            {
                processNode(treeView_stor.Nodes[0], storObj.get_el(j));
                j++;
            }
            //for (int i = 0; i < storObj.get_count(); ++i)
            //    processNode(treeView_stor.Nodes[0], storObj.get_el(i));
            // раскрываем главный узел дерева и только его
            //treeView_stor.Nodes[0].Expand();
        }
        private void treeView_stor_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // находим индекс элемента в хранилище
            int ind = treeView_stor.SelectedNode.Index;
            // выделяем его
            storObj.change_highlight_of_object(ind);
            pictureBox1.Invalidate();
        }

        private void btn_sticky_Click(object sender, EventArgs e)
        {
            storObj.set_sticky_of_objects(true);
            pictureBox1.Invalidate();
        }

        private void btn_nonSticky_Click(object sender, EventArgs e)
        {
            storObj.set_sticky_of_objects(false);
            pictureBox1.Invalidate();
        }
    }
    public class Brush
    {
        public static SolidBrush normBrush = new SolidBrush(Color.LightPink);
        public static SolidBrush highlightBrush = new SolidBrush(Color.Red);
        public static Pen normPen = new Pen(Color.LightPink, 3);
        public static Pen highlightPen = new Pen(Color.Red, 4);
        public static Color Color = Color.LightPink;


        public static HatchBrush stickyBrush = new HatchBrush(HatchStyle.LargeConfetti, Color.Black, Color.Pink);
    }
}