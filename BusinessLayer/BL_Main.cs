using SchoolGrades.BusinessObjects;
using SharedWinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using gamon;
using gamon.TreeMptt;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SchoolGrades
{
    internal partial class BusinessLayer: Form
    {
        public int indexCurrentDrawn = 0;

        public List<Student> currentStudentsList;
        public List<Student> eligiblesList = new List<Student>();

        List<frmLessons> listLessons = new List<frmLessons>();

        System.Media.SoundPlayer suonatore = new System.Media.SoundPlayer();

        frmMain frmMain = new frmMain();

        School school;

        private string schoolYear;

        //bool formInitializing = true;
        //bool firstTime = true;

        Student currentStudent;
        Question currentQuestion;
        GradeType currentGradeType;
        
        internal void randomNumber()
        {
            frmRandom f = new frmRandom();
            f.Show();
        }

        internal void costretto()
        {
            
            if (frmMain.currentClass != null)
            {
                frmMain.picStudent.Image = null;
                frmMain.beBrave();
            }
            else
        if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
                return;
            if (frmMain.chkSuspence.Checked)
            {
                int suspenceDelay = 4069; // in ms
                try
                {
                    suonatore.SoundLocation = ".\\Lo squalo.wav";
                    suonatore.Play();
                    Thread.Sleep(suspenceDelay);
                }
                catch
                {
                    Console.Beep(220, suspenceDelay);
                }
            }
            if (frmMain.currentClass.CurrentStudent != null)
            {
                frmMain.loadStudentsData(frmMain.currentClass.CurrentStudent);
            }
            else
            {

            }
        }

        internal void assess()
        {

            if (frmMain.currentClass is null)
            {
                MessageBox.Show("Selezionare una classe");
                return;
            }
            if (frmMain.cmbGradeType.Text == "")
            {
                MessageBox.Show("Selezionare un tipo di valutazione");
                return;
            }
            if (frmMain.currentClass.CurrentStudent is null)
            {
                MessageBox.Show("Selezionare un allievo da valutare");
                return;
            }
            if (frmMain.currentSubject == null)
            {
                MessageBox.Show("Selezionare una materia");
                return;
            }
            currentGradeType = ((GradeType)frmMain.cmbGradeType.SelectedItem);
            if (currentGradeType.IdGradeTypeParent == "")
            {
                MessageBox.Show("Con il tipo di valutazione scelto non si può fare la media.\r\n " +
                    "Selezionare un tipo di valutazione corretto");
                return;
            }
            frmMicroAssessment grade = new frmMicroAssessment(frmMain,
                frmMain.currentClass, frmMain.currentClass.CurrentStudent,
                currentGradeType, frmMain.currentSubject, frmMain.CurrentQuestion);
            //grade.ShowDialog();
            grade.Show();

            if (grade.CurrentQuestion != null)
            {
                frmMain.CurrentQuestion = grade.CurrentQuestion;
                frmMain.txtQuestion.Text = frmMain.CurrentQuestion.Text;
                frmMain.lstTimeInterval.Text = frmMain.CurrentQuestion.Duration.ToString();
                // start the timer if the question has a timer
                if (frmMain.CurrentQuestion.Duration != null && frmMain.CurrentQuestion.Duration > 0)
                {
                    frmMain.btnStartColorTimer_Click(null, null);
                }
            }
        }

        internal void question()
        {
            if (!CommonsWinForms.CheckIfSubjectChosen(frmMain.currentSubject))
                return;
            frmQuestionChoose scelta = new frmQuestionChoose(frmMain.currentSubject,
                frmMain.currentClass, currentStudent, frmMain.CurrentQuestion);
            scelta.ShowDialog();
            if (scelta.ChosenQuestion != null && scelta.ChosenQuestion.IdQuestion != 0)
            {
                frmMain.CurrentQuestion = scelta.ChosenQuestion;
            }
            scelta.Dispose();
        }

        internal void oldestGrade()
        {
            
            List<Couple> fromOldest = Commons.bl.GetGradesOldestInClass(frmMain.currentClass,
    ((GradeType)(frmMain.cmbGradeType.SelectedItem)), frmMain.currentSubject);
            Student trovato = null;
            int keyFirst = fromOldest[0].Key;
            foreach (Student s in currentStudentsList)
            {
                if (s.IdStudent == keyFirst)
                {
                    trovato = s;
                    break;
                }
            }
            if (trovato == null)
            {
                MessageBox.Show("Allievo con voticino più vecchio non trovato");
                return;
            }
            frmMain.currentClass.CurrentStudent = trovato;
            frmMain.loadStudentsData(frmMain.currentClass.CurrentStudent);
        }

        internal void topicsDone()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
                return;
            if (!CommonsWinForms.CheckIfSubjectChosen(frmMain.currentSubject))
                return;
            frmTopics frm = new frmTopics(frmTopics.TopicsFormType.HighlightTopics,
                frmMain.currentClass, frmMain.currentSubject, currentQuestion, null, frmMain);

            frm.Show();
        }

        internal void yearTopics()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
            {
                return;
            }
            if (!CommonsWinForms.CheckIfSubjectChosen(frmMain.currentSubject))
            {
                return;
            }
            string filenameNoExtension = frmMain.currentClass.SchoolYear +
                "_" + frmMain.currentClass.Abbreviation +
                "_" + frmMain.currentSubject.IdSchoolSubject + "_" +
                "all-topics";
            if (MessageBox.Show("Creare un file di testo normale (Sì) od un file per Markdown (No)?",
                "Tipo di file", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Commons.bl.CreateAllTopicsDoneFile(filenameNoExtension, frmMain.currentClass, frmMain.currentSubject, true);
                Commons.ProcessStartLink(Path.Combine(Commons.PathDatabase, filenameNoExtension + ".txt"));
                MessageBox.Show("Creato il file " + filenameNoExtension + ".txt");
            }
            else
            {
                Commons.bl.CreateAllTopicsDoneFile(filenameNoExtension, frmMain.currentClass, frmMain.currentSubject, false);
                Commons.ProcessStartLink(Path.Combine(Commons.PathDatabase, filenameNoExtension + ".md"));
                MessageBox.Show("Creato il file " + filenameNoExtension + ".md");
            }
        }

        internal void classesGradeSummary()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
            {
                return;
            }
            if (!CommonsWinForms.CheckIfSubjectChosen(frmMain.currentSubject))
            {
                return;
            }
            frmGradesClassSummary f;
            f = new frmGradesClassSummary(frmMain.currentClass,
                currentGradeType, frmMain.currentSubject);
            f.Show();
        }

        internal void studentGradesSummary()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
                return;
            if (!CommonsWinForms.CheckIfTypeOfAssessmentChosen(currentGradeType))
                return;
            if (!CommonsWinForms.CheckIfStudentChosen(currentStudent))
                return;

            // annotation applied to a single student
            frmGradesStudentsSummary f = new frmGradesStudentsSummary(currentStudent, frmMain.cmbSchoolYear.Text,
                currentGradeType, (SchoolSubject)frmMain.cmbSchoolSubject.SelectedItem);
            f.Show();
        }

        internal void lessonTopics()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
                return;
            if (!CommonsWinForms.CheckIfSubjectChosen(frmMain.currentSubject))
                return;
            // open read only the forms after the first. 
            if (listLessons.Count > 0)
            {
                // delete from listLessons those forms that have been closed
                int i = 0;
                while (i < listLessons.Count)
                {
                    frmLessons fl = listLessons[i];
                    if (fl.IsFormClosed)
                    {
                        listLessons.Remove(fl);
                        fl.Dispose();
                        i--;
                    }
                    i++;
                }
            }
            frmLessons flt;
            if (listLessons.Count == 0)
                flt = new frmLessons(frmMain.currentClass, (SchoolSubject)frmMain.cmbSchoolSubject.SelectedItem,
                    false);
            else
                flt = new frmLessons(frmMain.currentClass, (SchoolSubject)frmMain.cmbSchoolSubject.SelectedItem,
                    true);
            flt.Show();
            listLessons.Add(flt);
        }

        internal void studentNotes()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
                return;
            if (!CommonsWinForms.CheckIfTypeOfAssessmentChosen(currentGradeType))
                return;
            // annotation can be applied to a single student or to a whole list, based on the 
            // lstNames being visible or not 
            List<Student> chosenStudents;
            if (currentStudent != null && !frmMain.dgwStudents.Visible)
            {
                // annotation applied to a single student
                chosenStudents = new List<Student>();
                chosenStudents.Add(currentStudent);
            }
            else
            {
                // read checksigns from the grid
                frmMain.ReadCheckSignsIntoCurrentStudentsList();
                // annotation applied to a whole list of students
                chosenStudents = frmMain.FillListOfChecked(currentStudentsList);
            }
            if (chosenStudents.Count > 0)
            {
                frmAnnotationsAboutStudents f = new frmAnnotationsAboutStudents(chosenStudents, frmMain.cmbSchoolYear.Text);
                f.StartPosition = FormStartPosition.CenterParent;
                f.Show();
            }
        }

        internal void checkToggle()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
            {
                return;
            }
            if (frmMain.dgwStudents.Visible)
                frmMain.AllToggle();
        }

        internal void checkRevenge()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
            {
                return;
            }
            if (frmMain.dgwStudents.Visible)
                frmMain.AllCheckRevenge();
        }

        internal void checkNoGrade()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
            {
                return;
            }
            if (!CommonsWinForms.CheckIfSubjectChosen(frmMain.currentSubject))
            {
                return;
            }
            if (frmMain.dgwStudents.Visible)
                frmMain.AllCheckNonGraded();
        }

        internal void checkNone()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
            {
                return;
            }
            if (frmMain.dgwStudents.Visible)
                frmMain.AllUnChecked();
        }

        internal void checkAll()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
            {
                return;
            }
            if (frmMain.dgwStudents.Visible)
                frmMain.AllChecked();
        }

        internal void mosaic()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
                return;
            frmMosaic f = new frmMosaic(frmMain.currentClass);
            f.Show();
        }

        internal void startLinks()
        {
            if (!CommonsWinForms.CheckIfClassChosen(frmMain.currentClass))
                return;
            List<StartLink> LinksOfClass = Commons.bl.GetStartLinksOfClass(frmMain.currentClass);

            Commons.StartLinks(frmMain.currentClass, LinksOfClass);
        }
    }

}

