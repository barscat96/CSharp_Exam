using CSharp_Exam.Casino.Models;

namespace CSharp_Exam.Casino.Games;

public interface IGame
{
    public decimal Play(User user);
}