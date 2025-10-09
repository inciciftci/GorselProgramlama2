using System.Diagnostics.Metrics;

namespace giteYuklenecek
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Button btn1 = new Button();
            Button btn2 = new Button();
            Button btn3 = new Button();
            Button btn4 = new Button();

            this.Controls.Add(btn1);
            this.Controls.Add(btn2);
            this.Controls.Add(btn3);
            this.Controls.Add(btn4);

            btn1.SetBounds(350, 350, 50, 50);
            btn2.SetBounds(400, 350, 50, 50);
            btn3.SetBounds(350, 400, 50, 50);
            btn4.SetBounds(400, 400, 50, 50);


            for (; ; )
            {
                for (int i = 0; i < 71; i++)
                {


                    //11
                    btn1.Location = new Point(btn1.Location.X - 5, btn1.Location.Y - 5);


                    //12
                    btn2.Location = new Point(btn2.Location.X + 5, btn2.Location.Y - 5);


                    //21
                    btn3.Location = new Point(btn3.Location.X - 5, btn3.Location.Y + 5);



                    //22
                    btn4.Location = new Point(btn4.Location.X + 5, btn4.Location.Y + 5);

                    await Task.Delay(100);

                }
                for (int i = 0; i < 71; i++)
                {
                    btn1.Location = new Point(btn1.Location.X + 5, btn1.Location.Y + 5);
                    btn2.Location = new Point(btn2.Location.X - 5, btn2.Location.Y + 5);
                    btn3.Location = new Point(btn3.Location.X + 5, btn3.Location.Y - 5);
                    btn4.Location = new Point(btn4.Location.X - 5, btn4.Location.Y - 5);
                    await Task.Delay(100);
                }


            }
        }
    }
}
