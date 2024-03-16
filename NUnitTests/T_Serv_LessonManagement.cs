using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SchoolGrades;
using System.Threading;
using System;
using System.Collections.Generic;

namespace NUnitDbTests
{
    internal class T_Serv_LessonManagement
    {

        [SetUp]
        public void Setup()
        {
            Test_Commons.SetDataLayer();
        }

        [Test]
        public void T_Serv_LessonManagement_Create()
        {

            Test_Commons.dl.CreateTableLesson();
        }


        [Test]
        public void T_Serv_LessonManagement_Read()
        {
            Class Class = new Class();
            Lesson Lesson = new Lesson();
            Lesson CurrentLesson = new Lesson();
            DbDataReader dRead = new DbDataReader();
            string IdSubject = "Sela";
            DateTime Date = DateTime.Now;
            SchoolSubject Subject = new SchoolSubject();
            DateTime? DateStart = DateTime.Now;
            DateTime? DateFinish = DateTime.Now;

            Test_Commons.dl.GetLessonsOfClass(Class, Lesson);
            Test_Commons.dl.GetLessonFromRow(dRead);
            Test_Commons.dl.GetTopicsOfOneLessonOfClass(Class, Lesson);
            Test_Commons.dl.GetLastLesson(CurrentLesson);
            Test_Commons.dl.GetLessonInDate(Class, IdSubject, Date);
            Test_Commons.dl.GetLessonsImagesList(Lesson);
            Test_Commons.dl.GetTopicsDoneInClassInPeriod(Class, Subject, DateStart, DateFinish);

            {
            }

            [Test]
            public void T_Serv_LessonManagement_Update()
            {
                Lesson Lesson = new Lesson();
                Image Image = new Image();

                Test_Commons.dl.NewLesson(Lesson);
                Test_Commons.dl.SaveLesson(Lesson);
                Test_Commons.dl.LinkOneImageToLesson(Image, Lesson);

            }

            [Test]
            public void T_Serv_LessonManagement_Delete()
            {
                int? IdLesson = 12334;
                bool AlsoEraseImageFiles = true;
                Test_Commons.dl.EraseLesson(IdLesson, AlsoEraseImageFiles);
            }

        }
    }
}
