using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Student.Achieve.IRepository;
using Student.Achieve.Model.Models;
using Student.Achieve.Repository.Base;

namespace Student.Achieve.Repository
{
    public class ExamDetailScoreRepository : BaseRepository<ExamDetailScore>, IExamDetailScoreRepository
    {
    }
}
