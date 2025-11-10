using KDemia.Models;

namespace KDemia.Repositories
{
    public class CourseRepository
    {
        public static List<Course> courses = new List<Course> {
            new Course() { Id = 1, courseName = "MVC + C# ",  Description = "Kurs Açıklama 1", IsActive = true },
            new Course() { Id = 2, courseName = "Python", Description = "Kurs Açıklama ", IsActive = true },
            new Course() { Id = 3, courseName = "SQL", Description = "Kurs açıklama 3", IsActive = false },
            new Course() { Id = 4, courseName = "HTML & CSS", Description = "Kurs açıklama 4", IsActive = false },
            new Course() {Id=5,courseName="JAVA", Description="Kurs Açıklama 5", IsActive=false }
        };
        public List<Course>GetList()
        {
            return courses.ToList();
        }
        public Course GetById(int id)
        {
            var course = courses.Where(s => s.Id == id).FirstOrDefault();
            return course;
        }
        public void Add(Course model)
        {
            Random rn = new Random();
            int id = rn.Next(1000);
            model.Id = id;
            courses.Add(model);
        }
        public void Update(Course model)
        {
            var course = GetById(model.Id);
            if (course != null)
            {
                var index = courses.FindIndex(x => x.Id == course.Id);
                course.courseName = model.courseName;
                course.Description = model.Description;
                course.IsActive = model.IsActive;
                courses[index] = course;
            }
        }
        public void Delete(int id)
        {
            var course = GetById(id);
            if (course != null)
            {
                courses.Remove(course);
            }
        }
    }
}
