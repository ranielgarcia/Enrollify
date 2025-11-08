using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enhance_Genetic_Algorithm.Models;
public enum RoomType
{
    Regular,        // Any course
    ComputerLab,    // CS courses only
    ScienceLab,     // Science courses (Physics, Chemistry, Biology)
    LectureHall     // Large courses
}