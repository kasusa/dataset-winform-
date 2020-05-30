using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 学籍和成绩
{
    public partial class Form1 : Form
    {
        private DataSet dataSet;
        private OleDbConnection oleDbConnection;
        private OleDbDataAdapter oleDbDataAdapter;
        private OleDbCommandBuilder oleDbCommandBuilder;
        private DataRelation dataRelation;  //students和grade的关联
        private OleDbDataAdapter oleDbDataAdapter1;//用于grade表


        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ConnectDB(); LoadData(); BindingControls();
        }

        #region 加载数据,bind控件函数
        private void ConnectDB()
        {
            oleDbConnection = new OleDbConnection("Provider=SQLOLEDB.1;Data Source=101.200.157.164;Initial Catalog=zhp-winform;Persist Security Info=True;User ID=sa;Password=Jinghaoyang1");
            oleDbConnection.Open();
        }
        private void LoadData()
        {
            oleDbDataAdapter = new OleDbDataAdapter("select * from students", oleDbConnection);
            dataSet = new DataSet();
            oleDbDataAdapter.Fill(dataSet, "Students");
            oleDbCommandBuilder = new OleDbCommandBuilder(oleDbDataAdapter);//把adapter的信息放入commandBuilder
            oleDbDataAdapter.DeleteCommand = oleDbCommandBuilder.GetDeleteCommand();
            oleDbDataAdapter.UpdateCommand = oleDbCommandBuilder.GetUpdateCommand();
            oleDbDataAdapter.InsertCommand = oleDbCommandBuilder.GetInsertCommand();
            //toolStripStatusLabel1.Text = oleDbDataAdapter.SelectCommand;

            //DataSet本身没有从数据库加载数据的能力，必须通过OleDbDataAdapter提供的Fill方法进行加载。
            //在Fill过程中，若dataSet中没有students，则会自动建立，并把“select * from students”结果载入。
            oleDbDataAdapter.SelectCommand = new OleDbCommand("select * from classes", oleDbConnection);
            oleDbDataAdapter.Fill(dataSet, "Classes");

        }
        private void BindingControls()
        {
            //绑定textbox 学号
            textBox1.DataBindings.Add("text", dataSet, "students.id");
            //绑定textbox 姓名
            textBox2.DataBindings.Add("text", dataSet, "students.name");
            //绑定combobox 下拉列表
            comboBox1.DataSource = dataSet.Tables["classes"]; ;
            comboBox1.DisplayMember = "name";
            comboBox1.ValueMember = "id";
            comboBox1.DataBindings.Add("selectedValue", dataSet, "students.classid");

        }
        private void LoadGrade()
        {
            oleDbDataAdapter1 = new OleDbDataAdapter();
            oleDbDataAdapter1.SelectCommand = new OleDbCommand("select a.id,a.subid,b.subname,a.grade from grade a,subjects b where a.subid=b.subid", oleDbConnection);
            oleDbDataAdapter1.Fill(dataSet, "grade");//dataSet中产生grade表
                                                     //update时调用的update语句，其中“？”表示定位参数（非命名参数），依次和//下面oleDbDataAdapter1.UpdateCommand.Parameters中的参数对应。
            oleDbDataAdapter1.UpdateCommand = new OleDbCommand("update grade set grade=? where id=? and subid=?", oleDbConnection);
            //以下语句定义执行update时三个参数来自dataSet中grade（由fill语句的二个参数确定）表的哪个列
            //定义第一个参数：名为“grade”,对定位参数，参数按次序对应，名称无用
            OleDbParameter gradeParameter = new OleDbParameter("grade", OleDbType.Integer);
            //该参数数据来源grade列
            gradeParameter.SourceColumn = "grade";
            //数据取修改后数据，此语句可省略
            gradeParameter.SourceVersion = DataRowVersion.Current;//缺省
                                                                  //定义第二个参数id：数据取修改前数据
            OleDbParameter idParameter = new OleDbParameter("id", OleDbType.Char, 6);
            idParameter.SourceColumn = "id";  //数据来源于id列
            idParameter.SourceVersion = DataRowVersion.Original;
            //定义第三个参数subid：数据取修改前数据，同时指定数据来源于subid列
            OleDbParameter subidParameter = new OleDbParameter("subid", OleDbType.Char, 6, "subid");
            subidParameter.SourceVersion = DataRowVersion.Original;
            //将三个参数依次加入参数表，作为update语句中的三个参数
            oleDbDataAdapter1.UpdateCommand.Parameters.Add(gradeParameter);
            oleDbDataAdapter1.UpdateCommand.Parameters.Add(idParameter);
            oleDbDataAdapter1.UpdateCommand.Parameters.Add(subidParameter);
            //建立名为students_grade的两表关系，联结条件students.id=grade.id
            dataRelation = new DataRelation("students_grade", dataSet.Tables["students"].Columns["id"], dataSet.Tables["grade"].Columns["id"]);
            //将关系加入dataSet.Relation中
            dataSet.Relations.Add(dataRelation);
        }

        #endregion

        #region 上一条下一条
        private void buttonR_Click(object sender, EventArgs e)
        {
            this.BindingContext[dataSet, "students"].Position += 1;
        }

        private void buttonL_Click(object sender, EventArgs e)
        {
            this.BindingContext[dataSet, "students"].Position -= 1;
        }

        private void buttonLL_Click(object sender, EventArgs e)
        {
            this.BindingContext[dataSet, "students"].Position = 0;
        }

        private void buttonRR_Click(object sender, EventArgs e)
        {
            //我发现,即使你给这个position赋值超过范围,也不会报错,会找到最大的.
            this.BindingContext[dataSet, "students"].Position = int.MaxValue;
        }
        #endregion

        #region 功能按钮
        private void button_insert_Click(object sender, EventArgs e)
        {
            this.BindingContext[dataSet, "students"].AddNew();
        }

        private void button_del_Click(object sender, EventArgs e)
        {
            this.BindingContext[dataSet, "students"].RemoveAt(this.BindingContext[dataSet, "students"].Position);
        }

        private void button_cancle_Click(object sender, EventArgs e)
        {
            this.BindingContext[dataSet, "students"].CancelCurrentEdit();
        }

        private void button_cancelAll_Click(object sender, EventArgs e)
        {
            dataSet.RejectChanges();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            this.BindingContext[dataSet, "students"].EndCurrentEdit();
            oleDbDataAdapter.Update(dataSet, "students");
            dataSet.AcceptChanges();
        }
        #endregion
    }
}
