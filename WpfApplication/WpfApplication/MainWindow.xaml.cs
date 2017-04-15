using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Threading;

namespace WpfApplication
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    public partial class MainWindow : Window
    {
        const bool DEBUG = true;
        public static String current_mode = BLUR;
        public const String BLUR = "blur";
        public const String REPLACE = "replace";
        public const String DELETE = "delete";
        public const String FILTER = "filter";
        public List<String> delete_img_path = new List<String>();

        private Button btn_replace_no_image = null;
        private Button btn_replace_add_image = null;

        private Button btn_delete_no_image = null;
        private Button btn_delete_add_image = null;

        private ScrollViewer scroll_replace = null;
        private ScrollViewer scroll_delete = null;

        private Grid grid_blur = null;
        private Image grid_blur_im = null;
        private StackPanel grid_blur_panel = null;
        private Dictionary<int, Image> grid_blur_im_cache = new Dictionary<int, Image>();
        private Slider grid_blur_silder = null;
        private TextBox grid_blur_text_box = null;

        public MainWindow()
        {
            InitializeComponent();
            init_grid_func_select();
            btn_blur_Click(null, null);
            init_btn_no_image();
            init_btn_add_image();
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, EventArgs e)
        {
            grid_blur.Children.Clear();
            foreach(String filename in delete_img_path)
            {
                System.IO.File.Delete(filename);
            }
        }

        public void init_grid_blur()
        {
            if(grid_blur == null)
            {
                grid_blur_panel = new StackPanel();
                grid_blur_panel.VerticalAlignment = VerticalAlignment.Center;
                Image im = get_im_by_path(ConstResource.blur_im_path + "\\0.jpg");
                grid_blur_im = im;

                grid_blur_im_cache.Add(0, im);

                grid_blur_panel.Children.Add(grid_blur_im);
                grid_blur_silder = new Slider();
                grid_blur_silder.Orientation = Orientation.Horizontal;
                grid_blur_silder.BorderThickness = new System.Windows.Thickness(10);
                grid_blur_silder.Maximum = 14;
                grid_blur_silder.Minimum = 0;
                grid_blur_silder.Margin = new System.Windows.Thickness(20, 10, 20, 10);
                grid_blur_silder.IsSnapToTickEnabled = true;
                grid_blur_silder.TickPlacement = TickPlacement.BottomRight;
                grid_blur_silder.ValueChanged += grid_blur_silder_value_changed;


                grid_blur_panel.Children.Add(grid_blur_silder);
                grid_blur_text_box = new TextBox();
                grid_blur_text_box.Text = "关闭";
                grid_blur_text_box.IsEnabled = false;
                
                grid_blur_text_box.BorderBrush = Brushes.Transparent;
                grid_blur_text_box.TextAlignment = TextAlignment.Center;
                grid_blur_panel.Children.Add(grid_blur_text_box);
                grid_blur = new Grid();
                grid_blur.Margin = new System.Windows.Thickness(20);
                grid_blur.Children.Add(grid_blur_panel);

            }
            grid_func.Children.Clear();
            grid_func.Children.Add(grid_blur);
        }

        private void grid_blur_silder_value_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int blur_weight = Convert.ToInt32(grid_blur_silder.Value);
            if (blur_weight == 0)
            {
                grid_blur_text_box.Text = "关闭";
            }
            else
            {
                grid_blur_text_box.Text = blur_weight.ToString();
            }
            grid_blur_panel.Children.Remove(grid_blur_im);
            try
            {
                grid_blur_im = grid_blur_im_cache[blur_weight];
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Image tp_im = get_im_by_path(
                    System.IO.Path.Combine(System.Environment.CurrentDirectory,
                                     ConstResource.blur_im_path,
                                     blur_weight.ToString() + ".jpg")
                    );
                grid_blur_im_cache.Add(blur_weight, tp_im);
                grid_blur_im = grid_blur_im_cache[blur_weight];
            }
            grid_blur_panel.Children.Insert(0, grid_blur_im);
            if (DEBUG)
            {
                MessageBox.Show(EventHandler.event_handler(EventHandler.BLUR, blur_weight.ToString()));
            }
            else
            {
                EventHandler.event_handler_anys(EventHandler.BLUR, blur_weight.ToString());
            }
        }

        private void init_scroll_replace()
        {
            if(scroll_replace == null)
            {
                scroll_replace = new ScrollViewer();
                scroll_replace.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                String directory_path = System.IO.Path.Combine(System.Environment.CurrentDirectory, ConstResource.replace_im_path);
                DirectoryInfo d_info = new DirectoryInfo(directory_path);
                AutoBtnPanel btn_panel = new AutoBtnPanel();
                btn_panel.add_btn(btn_replace_no_image);
                foreach (FileInfo NextFile in d_info.GetFiles())
                {
                    PathBtn btn = new PathBtn();
                    btn.Content = get_im_by_path(NextFile.FullName);

                    btn.set_path(NextFile.FullName);
                    btn.Click += btn_replace_item_click;
                    btn.MouseRightButtonDown += btn_replace_item_right_click;
                    btn_panel.add_btn(btn);
                }
                btn_panel.add_btn(btn_replace_add_image);
                scroll_replace.Content = btn_panel;
            }
        }

        private void btn_replace_item_right_click(object sender, MouseButtonEventArgs e)
        {
            MenuItem menu_item = new MenuItem();
            menu_item.Header = "删除";
            menu_item.Click += delete_menu_item_Click;
            ContextMenu c_menu = new ContextMenu();
            c_menu.Items.Add(menu_item);
            ((Button)sender).ContextMenu = c_menu;

        }

        private void delete_menu_item_Click(object sender, RoutedEventArgs e)
        {
            Button bp = (Button)ContextMenuService.GetPlacementTarget(
                                LogicalTreeHelper.GetParent(sender as MenuItem)
                                );
            AutoBtnPanel auto_btn_panel = null;
            if (current_mode.Equals(REPLACE))
            {
                auto_btn_panel = (AutoBtnPanel)scroll_replace.Content;
                PathBtn bp_ = (PathBtn)bp;
                delete_img_path.Add(bp_.get_path());
            }else if (current_mode.Equals(DELETE))
            {
                ColorBtn bp_ = (ColorBtn)bp;
                auto_btn_panel = (AutoBtnPanel)scroll_delete.Content;
                String delete_path = System.IO.Path.Combine(
                    System.Environment.CurrentDirectory,
                    ConstResource.color_path,
                    bp_.get_color_name()
                    );
                delete_img_path.Add(delete_path);
            }
            auto_btn_panel.delete_btn(bp);
        }

        private void init_scroll_delete()
        {
            if (scroll_delete == null)
            {
                scroll_delete = new ScrollViewer();
                scroll_delete.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                AutoBtnPanel btn_panel = new AutoBtnPanel();
                btn_panel.add_btn(btn_delete_no_image);

                String directory_path = System.IO.Path.Combine(System.Environment.CurrentDirectory, ConstResource.color_path);
                DirectoryInfo d_info = new DirectoryInfo(directory_path);
                foreach (FileInfo NextFile in d_info.GetFiles())
                {
                    try {
                        string[] lines = System.IO.File.ReadAllLines(NextFile.FullName);
                        Color color = new Color();
                        color.A = Convert.ToByte(Convert.ToInt32(lines[0]));
                        color.R = Convert.ToByte(Convert.ToInt32(lines[1]));
                        color.G = Convert.ToByte(Convert.ToInt32(lines[2]));
                        color.B = Convert.ToByte(Convert.ToInt32(lines[3]));
                        ColorBtn btn = new ColorBtn(color);
                        btn.Height = grid_func.ActualWidth / 3;                       
                        btn.set_color_name(NextFile.Name);
                        btn.Click += btn_delete_item_click;
                        btn.MouseRightButtonDown += btn_delete_item_right_click;
                        btn_panel.add_btn(btn);
                    }
                    catch
                    {

                    }
                }
                btn_panel.add_btn(btn_delete_add_image);
                scroll_delete.Content = btn_panel;
            }
        }

        private void btn_delete_item_right_click(object sender, MouseButtonEventArgs e)
        {
            MenuItem menu_item = new MenuItem();
            menu_item.Header = "删除";
            menu_item.Click += delete_menu_item_Click;
            ContextMenu c_menu = new ContextMenu();
            c_menu.Items.Add(menu_item);
            ((Button)sender).ContextMenu = c_menu;
        }

        private void btn_delete_item_click(object sender, RoutedEventArgs e)
        {
            ColorBtn trigger_btn = (ColorBtn)sender;
            String trigger_btn_color = trigger_btn.color_name;

            //replace the inner image in the btn_replace
            //release the btn_replace content
            Grid btn_delete_grid = (Grid)btn_delete.Content;
            foreach (UIElement elem in btn_delete_grid.Children)
            {
                if (elem is Image)
                {
                    ColorBtn c_btn = new ColorBtn(trigger_btn.btn_color);
                    c_btn.Height = ((Image)elem).ActualHeight;
                    c_btn.Width = ((Image)elem).ActualWidth;
                    btn_delete_grid.Children.Remove(elem);
                    btn_delete_grid.Children.Insert(0, c_btn);
                    break;
                }
            }
            if (DEBUG)
            {
                MessageBox.Show(EventHandler.event_handler(EventHandler.COLOR, trigger_btn.btn_color.ToString()));
            }
            else
            {
                EventHandler.event_handler_anys(EventHandler.COLOR, trigger_btn.btn_color.ToString());
            }
        }

        private void init_grid_func(String func_name)
        {
            switch (func_name)
            {
                case REPLACE:
                    init_grid_replace();
                    break;
                default:
                    break;
            }
        }
        private void init_btn_add_image()
        {
            if (btn_replace_add_image == null)
            {
                btn_replace_add_image = new PathBtn();
                btn_replace_add_image.Click += btn_add_image_Click;
                btn_replace_add_image.Content = get_im_by_path(
                    System.IO.Path.Combine(System.Environment.CurrentDirectory, ConstResource.btn_add_im_path)
                    );

                btn_delete_add_image = new PathBtn();
                btn_delete_add_image.Click += btn_add_image_Click;
                btn_delete_add_image.Content = get_im_by_path(
                    System.IO.Path.Combine(System.Environment.CurrentDirectory, ConstResource.btn_add_im_path)
                    );
            }
        }

        private void init_btn_no_image()
        {
            if(btn_replace_no_image == null)
            {
                
                btn_replace_no_image = new PathBtn();
                btn_replace_no_image.Click += btn_no_image_Click;
                btn_replace_no_image.Content = get_im_by_path(
                    System.IO.Path.Combine(System.Environment.CurrentDirectory, ConstResource.btn_delete_im_path)
                    );

                btn_delete_no_image = new PathBtn();
                btn_delete_no_image.Click += btn_no_image_Click;
                btn_delete_no_image.Content = get_im_by_path(
                    System.IO.Path.Combine(System.Environment.CurrentDirectory, ConstResource.btn_delete_im_path)
                    );
            }
        }

        private void init_grid_delete()
        {
            grid_func.Children.Clear();
            init_scroll_delete();
            grid_func.Children.Add(scroll_delete);
        }
        private void init_grid_replace()
        {
            grid_func.Children.Clear();
            init_scroll_replace();
            grid_func.Children.Add(scroll_replace);
        }
        //param sender is the obj that trigger the event
        private void btn_replace_item_click(object sender, RoutedEventArgs e)
        {
            PathBtn trigger_btn = (PathBtn)sender;
            String trigger_btn_path = trigger_btn.get_path();
            
            //replace the inner image in the btn_replace
            //release the btn_replace content
            Grid btn_replace_grid = (Grid)btn_replace.Content;
            foreach (UIElement elem in btn_replace_grid.Children)
            {
                if(elem is Image)
                {
                    btn_replace_grid.Children.Remove(elem);
                    Image im = get_im_by_path(trigger_btn_path);
                    btn_replace_grid.Children.Insert(0, im);
                    break;
                }
            }
            if (DEBUG)
            {
                MessageBox.Show(EventHandler.event_handler(EventHandler.BACKGROUND,trigger_btn_path));
            }
            else
            {
                EventHandler.event_handler_anys(EventHandler.BACKGROUND, trigger_btn_path);
            }
        }

        //在这里填写no_image点击事件响应
        private void btn_no_image_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("set the path as none");
        }
        //在这里填写add_image点击事件响应
        private void btn_add_image_Click(object sender, RoutedEventArgs e)
        {
            if (current_mode.Equals(REPLACE))
            {
                replace_add();
            }else if (current_mode.Equals(DELETE))
            {
                delete_add();    
            }
        }
        private void delete_add()
        {
            System.Windows.Forms.ColorDialog color_dlg = new System.Windows.Forms.ColorDialog();
            if (color_dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color color = color_dlg.Color;
                int argb = color.ToArgb();
                byte[] b = new byte[4];
                b[0] = (byte)argb;
                b[1] = (byte)(argb >> 8);
                b[2] = (byte)(argb >> 16);
                b[3] = (byte)(argb >> 24);
                Color c = Color.FromArgb(b[3], b[2], b[1], b[0]);
                ColorBtn c_btn = new ColorBtn(c);
                c_btn.Height = btn_delete_no_image.ActualHeight;
                c_btn.Click += btn_delete_item_click;
                c_btn.MouseRightButtonDown += btn_delete_item_right_click;
                //insert the path btn
                AutoBtnPanel auto_btn_panel = (AutoBtnPanel)scroll_delete.Content;
                //remove the add image btn;
                auto_btn_panel.pop_btn();
                auto_btn_panel.add_btn(c_btn);
                auto_btn_panel.add_btn(btn_delete_add_image);

                //save the color file
                String save_path = System.IO.Path.Combine(
                    System.Environment.CurrentDirectory,
                    ConstResource.color_path, 
                    c_btn.get_color_name()
                    );
                try {
                    FileStream f = System.IO.File.Create(save_path);
                    f.Close();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    System.IO.File.Delete(save_path);
                    FileStream f = System.IO.File.Create(save_path);
                    f.Close();
                }
                finally
                {
                    using (System.IO.StreamWriter file =
                            new System.IO.StreamWriter(save_path))
                    {
                        file.WriteLine(b[3].ToString());
                        file.WriteLine(b[2].ToString());
                        file.WriteLine(b[1].ToString());
                        file.WriteLine(b[0].ToString());
                    }
                }
                
            }

        }
        private void replace_add()
        {
            //init an path btn
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "添加背景图片";
            ofd.Filter = "照片(*.jpg;*.jpeg;*.png;*.tiff;*.bmp)|*.jpg;*.jpeg;*.png;*.tiff;*.bmp";
            ofd.ShowDialog(this);
            if (System.IO.File.Exists(ofd.FileName))
            {
                PathBtn btn = new PathBtn();
                btn.set_path(ofd.FileName);
                btn.Content = get_im_by_path(btn.get_path());
                btn.Click += btn_replace_item_click;
                btn.MouseRightButtonDown += btn_replace_item_right_click;
                //insert the path btn
                AutoBtnPanel auto_btn_panel = (AutoBtnPanel)scroll_replace.Content;
                //remove the add image btn;
                auto_btn_panel.pop_btn();
                auto_btn_panel.add_btn(btn);
                auto_btn_panel.add_btn(btn_replace_add_image);
                try
                {
                    //copy the image to the res file
                    System.IO.File.Copy(
                        btn.get_path(),
                        System.IO.Path.Combine(
                                System.Environment.CurrentDirectory,
                                ConstResource.replace_im_path,
                                System.IO.Path.GetFileName(btn.get_path())
                            ),
                        true
                        );
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }

            }
        }
        //-------------------------------------------------------------------
        // 初始化功能导航栏
        //-------------------------------------------------------------------
        private void init_grid_func_select()
        {
            btn_blur.Content = get_btn_content_by_path(
                ConstResource.grid_func_im_path[ConstResource.BLUR_INDEX]
                );
            btn_replace.Content = get_btn_content_by_path(
                ConstResource.grid_func_im_path[ConstResource.REPLACE_INDEX]
                );
            btn_delete.Content = get_btn_content_by_path(
                ConstResource.grid_func_im_path[ConstResource.DELETE_INDEX]
                );
//             btn_filter.Content = get_btn_content_by_path(
//                 ConstResource.grid_func_im_path[ConstResource.FILTER_INDEX]
//                 );
        }
        private Grid get_btn_content_by_path(String grid_im_path)
        {
            Grid tp_grid = new Grid();
            Image im = get_im_by_path(grid_im_path);
            tp_grid.Children.Add(im);
            
            String filename = System.IO.Path.GetFileNameWithoutExtension(grid_im_path);
            TransparentTextBox box = new TransparentTextBox(filename);
            tp_grid.Children.Add(box);
            return tp_grid;
        }
        private Image get_im_by_path(String path)
        {
            Image image = new Image();
            if (!System.IO.Directory.Exists(path))
            {
                path = System.IO.Path.Combine(System.Environment.CurrentDirectory, path);
            }
            // Read byte[] from png file
            BinaryReader binReader = new BinaryReader(File.Open(path, FileMode.Open));
            FileInfo fileInfo = new FileInfo(path);
            byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
            binReader.Close();

            // Init bitmap
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(bytes);
            bitmap.EndInit();
            image.Source = bitmap;
            return image;
        }

        private void btn_blur_Click(object sender, RoutedEventArgs e)
        {
            btn_blur.Margin = new Thickness(0);
            btn_replace.Margin = new Thickness(20);
            btn_delete.Margin = new Thickness(20);
            /*btn_filter.Margin = new Thickness(20);*/
            current_mode = BLUR;
            init_grid_blur();
        }

        private void btn_replace_Click(object sender, RoutedEventArgs e)
        {
            btn_blur.Margin = new Thickness(20);
            btn_replace.Margin = new Thickness(0);
            btn_delete.Margin = new Thickness(20);
            /*btn_filter.Margin = new Thickness(20);*/
            init_grid_replace();
            current_mode = REPLACE;
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            btn_blur.Margin = new Thickness(20);
            btn_replace.Margin = new Thickness(20);
            btn_delete.Margin = new Thickness(0);
            /*btn_filter.Margin = new Thickness(20);*/
            init_grid_delete();
            current_mode = DELETE;
        }

        private void btn_filter_Click(object sender, RoutedEventArgs e)
        {
            btn_blur.Margin = new Thickness(20);
            btn_replace.Margin = new Thickness(20);
            btn_delete.Margin = new Thickness(20);
            /*btn_filter.Margin = new Thickness(0);*/
            current_mode = FILTER;
        }
       
    }
    //---------------------------------------------------------
    //some static resoure path
    //---------------------------------------------------------
    public static class ConstResource
    {
        public static String[] grid_func_im_path = { "res\\func_img\\模糊.jpg",
            "res\\func_img\\替换.jpg", "res\\func_img\\删除.png", "res\\func_img\\滤镜.png" };
        public static String blur_im_path = "res\\smart_blur";
        public static String color_path = "res\\color";
        public static String replace_im_path = "res\\replace";
        public const String btn_add_im_path = "res\\func_img\\Add.png";
        public const String btn_delete_im_path = "res\\func_img\\NoImage.png";
        public const int REPLACE_INDEX = 1;
        public const int BLUR_INDEX = 0;
        public const int DELETE_INDEX = 2;
        public const int FILTER_INDEX = 3;
    }
    //---------------------------------------------------------
    // in order to help insert btn and remove btn in the 
    // grid panel
    //---------------------------------------------------------
    public class AutoBtnPanel : StackPanel
    {
        int cur_btn_number = 0;
        const int coloumns = 3;
        List<Button> btn_list = new List<Button>();
        List<UniformGrid> row_set = new List<UniformGrid>();
        public AutoBtnPanel()
        {
//             UniformGrid e = elem();
//             Children.Add(e);
//             row_set.Add(e);
        }
        public void add_btn(Button btn)
        {
            if(cur_btn_number % coloumns == 0)
            {
                UniformGrid e = elem();
                Children.Add(e);
                row_set.Add(e);
            }
            row_set[row_set.Count - 1].Children.Add(btn);
            btn_list.Add(btn);
            cur_btn_number++;
        }
        public void pop_btn()
        {
            cur_btn_number--;

            row_set[row_set.Count - 1].Children.RemoveAt(cur_btn_number % coloumns);
            
            if (cur_btn_number % coloumns == 0)
            {
                row_set.RemoveAt(row_set.Count - 1);
            }
            btn_list.RemoveAt(btn_list.Count - 1);
        }
        private void pop_btn_in_row_set()
        {
            cur_btn_number--;
            row_set[row_set.Count - 1].Children.RemoveAt(cur_btn_number % coloumns);

            if (cur_btn_number % coloumns == 0)
            {
                Children.Remove(row_set[row_set.Count - 1]);
                row_set.RemoveAt(row_set.Count - 1);
            }
        }

        private void add_btn_to_row_set(Button btn)
        {
            if (cur_btn_number % coloumns == 0)
            {
                UniformGrid e = elem();
                Children.Add(e);
                row_set.Add(e);
            }
            row_set[row_set.Count - 1].Children.Add(btn);
            cur_btn_number++;
        }
        public void delete_btn(Button btn)
        {
            delete_btn(btn_list.IndexOf(btn));
        }
        public void delete_btn(int index)
        {
            for(int i = 0; i < btn_list.Count - index; i++)
            {
                pop_btn_in_row_set();
            }
            btn_list.RemoveAt(index);
            for (int i = index; i < btn_list.Count; i++)
            {
                add_btn_to_row_set(btn_list[i]);
            }
        }
        //ret a uniform grid which row = 1 and col = coloums
        public UniformGrid elem()
        {
            UniformGrid grid_ret = new UniformGrid();
            grid_ret.Rows = 1;
            grid_ret.Columns = coloumns;
            return grid_ret;
        }
    }
    //---------------------------------------------------------
    // the button under the replace background func grid
    //---------------------------------------------------------
    public class PathBtn : Button
    {
        private String path;
        public PathBtn()
        {
//             < Setter Property = "Background" Value = "Transparent" />
//                < Setter Property = "BorderBrush" Value = "Transparent" />
//                      < Setter Property = "Padding" Value = "2" />
            Margin = new Thickness(2);
            Background = Brushes.Transparent;
            BorderBrush = Brushes.Transparent;
            
        }

        public String get_path()
        {
            return path;
        }
        public void set_path(String path_)
        {
            path = path_;
            String filename = System.IO.Path.GetFileNameWithoutExtension(path);
            ToolTip tp = new ToolTip();
            tp.Content = filename;
            this.ToolTip = tp;
        }
    }
    //---------------------------------------------------------
    // the button under the delete func grid
    //---------------------------------------------------------
    public class ColorBtn : Button
    {
        public Color btn_color;
        public String color_name;
        public ColorBtn(Color color)
        {
            btn_color = color;
            Background = new SolidColorBrush(color);
            Margin = new Thickness(2);
            BorderBrush = Brushes.Transparent;
            color_name = color.ToString();
            ToolTip tooltip = new ToolTip();
            tooltip.Content = color_name;
            this.ToolTip = tooltip;
        }
        public void set_color_name(String name)
        {
            ToolTip tooltip = new ToolTip();
            tooltip.Content = name;
            color_name = name;
            this.ToolTip = tooltip;
        }
        public String get_color_name()
        {
            return color_name;
        }
    }
    public class TransparentTextBox : TextBox
    {
        public TransparentTextBox(String line_text)
        {
            this.Text = line_text;
            this.TextAlignment = TextAlignment.Center;
            this.Foreground = Brushes.White;
            this.Background = Brushes.Gray;
            this.BorderThickness = new System.Windows.Thickness(0);
            this.IsEnabled = false;
            this.Opacity = 90;
            this.FontFamily = new System.Windows.Media.FontFamily("宋体");
            this.FontSize = 10;
            this.Height = this.FontSize;
            this.VerticalAlignment = VerticalAlignment.Bottom;
        }
    }
    public static class CmdProcessHelper
    {
        public static String exec_cmd(string cmd)
        {

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(cmd);

            p.StandardInput.AutoFlush = true;
            p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令
            //获取cmd窗口的输出信息
            string output = p.StandardOutput.ReadToEnd();

//             StreamReader reader = p.StandardOutput;
//             string line=reader.ReadLine();
//             while (!reader.EndOfStream)
//             {
//                 str += line + "  ";
//                 line = reader.ReadLine();
//             }

            p.WaitForExit();//等待程序执行完退出进程
            p.Close();

            return output;
        }
        //-----------------------------------------------------------------
        // 新建线程异步处理，所以不返回结果, delay表示等待多久后开启线程处理任务
        //-----------------------------------------------------------------
        public static void exec_cmd_ayns(string cmd, int delay=0)
        {
            Thread thread = new Thread(exec_cmd);
            //构造参数
            object[] p_ = new object[2];
            p_[0] = cmd;
            p_[1] = delay;
            thread.Start(p_);
        }
        private static void exec_cmd(object cmd)
        {
            //读取参数
            object[] p_ = (object[])cmd;
            int delay = (int)p_[1];
            String cmd_ = (String)p_[0];
            Thread.Sleep(delay);

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.AutoFlush = true;
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
        }
    }
    //--------------------------------------------------------
    // handle the event interact with BKG_EXE
    //--------------------------------------------------------
    public static class EventHandler
    {
        public const String BKG_EXE = "BackgroundHelper.exe";
        public const String BLUR = " -b ";
        public const String BACKGROUND = " -p ";
        public const String COLOR = " -c ";
        public const String FILTER = " -f ";
        const int MAX_THREAD_NUM = 1;
        static int cur_thread_num = 0;
        public static String event_handler(String event_, String param){
            pre_handle_brefore_call_bkg_exe();
            switch (event_){
                case BLUR:
                    return blur_backgroud(Convert.ToInt32(param));
                case BACKGROUND:
                    return change_background_image(param);
                case COLOR:
                    return change_background_color(param);
                case FILTER:
                    return filte_background(param);
                default:
                    return "event exception";
            }
        }
        public static void event_handler_anys(String event_, String param, int delay = 0)
        {
            if (cur_thread_num < MAX_THREAD_NUM)
            {
                Thread thread = new Thread(event_handler);
                object[] obj_ = new object[3];
                obj_[0] = event_;
                obj_[1] = param;
                obj_[2] = delay;
                thread.Start(obj_);
            }
        }

        public static void event_handler(object obj)
        {
            cur_thread_num++;

            //读取参数
            object[] p_ = (object[])obj;
            String event_ = (String)p_[0];
            String param = (String)p_[1];
            int delay = (int)p_[2];
            
            Thread.Sleep(delay);
            pre_handle_brefore_call_bkg_exe();
            switch (event_)
            {
                case BLUR:
                    blur_backgroud(Convert.ToInt32(param));
                    break;
                case BACKGROUND:
                    change_background_image(param);
                    break;
                case COLOR:
                    change_background_color(param);
                    break;
                case FILTER:
                    filte_background(param);
                    break;
                default:
                    break;
            }

            cur_thread_num--;
        }
        private static String change_background_image(String abs_path)
        {
            return CmdProcessHelper.exec_cmd(BKG_EXE + " -p " + abs_path);
        }
        private static String change_background_color(String color_name)
        {
            try {//color_name  have format #ARGB like "#FFF0F8FF" and should return the Format RGB
                color_name = color_name.Substring(3, 6);
            }catch(System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            return CmdProcessHelper.exec_cmd(BKG_EXE + " -c " + color_name);
        }
        private static String blur_backgroud(int blur_weight)
        {
            return CmdProcessHelper.exec_cmd(BKG_EXE + " -b " + blur_weight.ToString());
        }
        private static String filte_background(String filter_flag)
        {
            return CmdProcessHelper.exec_cmd(BKG_EXE + " -f " + filter_flag);
        }

//         private static void change_background_image_ayns(String abs_path)
//         {
//             CmdProcessHelper.exec_cmd_ayns(BKG_EXE + " -p " + abs_path);
//         }
//         private static void change_background_color_ayns(String color_name)
//         {
//             //color_name  have format #ARGB like "#FFF0F8FF" and should return the Format RGB
//             color_name.Remove(0);
//             color_name.Remove(0);
//             color_name.Remove(0);
//             CmdProcessHelper.exec_cmd_ayns(BKG_EXE + " -c " + color_name);
//         }
//         private static void blur_backgroud_ayns(int blur_weight)
//         {
//             CmdProcessHelper.exec_cmd_ayns((BKG_EXE + " -b " + blur_weight.ToString()));
//         }
//         private static void filte_background_ayns(String filter_flag)
//         {
//             CmdProcessHelper.exec_cmd_ayns((BKG_EXE + " -f " + filter_flag));
//         }

        private static bool is_bkg_exe()
        {
            String ret = CmdProcessHelper.exec_cmd("tasklist | findstr " + BKG_EXE);
            if(ret != null && ret.Length > 0)
            {
                return true;
            }
            return false;
        }
        private static void kill_bkg_exe()
        {
            CmdProcessHelper.exec_cmd("tskill " + BKG_EXE.Split('.')[0]);
        }
        //if the bkg_exe is already run first kill the process
        private static void pre_handle_brefore_call_bkg_exe()
        {
            if (is_bkg_exe())
            {
                kill_bkg_exe();
            }
        }
    }
}
