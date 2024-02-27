using Burmuruk.Tesis.Control;

namespace Assets.Proyect.Scripts.Control
{
    public class AIEnemyController : Character
    {
        protected override void DecisionManager()
        {
            base.DecisionManager();

            if (isClose)
            {
                print("in range");
            }
            if (isFar)
            {
                print("In sight");
            }
        }
    }
}
