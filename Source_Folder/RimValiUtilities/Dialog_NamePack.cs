using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{

    public class Dialog_NamePack : Window
    {

        private string curPackName;
        private readonly Pawn pawn;

        public override Vector2 InitialSize => new Vector2(500f, 175f);


        public Dialog_NamePack(Pawn pawn)
        {
            this.pawn = pawn;
            curPackName = pawn.GetPack().name;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            closeOnAccept = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                flag = true;
                Event.current.Use();
            }
            Text.Font = GameFont.Medium;
            string text = "PackNameLabel".Translate(pawn.GetPack().name.Named("PACKNAME"));
            Widgets.Label(new Rect(15f, 15f, 500f, 50f), text);
            Text.Font = GameFont.Small;
            string text2 = Widgets.TextField(new Rect(15f, 50f, inRect.width / 2f - 20f, 35f), curPackName);
            if (text2.Length < 30 && CharacterCardUtility.ValidNameRegex.IsMatch(text2)) { curPackName = text2; }
            if (Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "OK", true, true, true) || flag)
            {
                if (string.IsNullOrEmpty(curPackName)) { curPackName = pawn.GetPack().name; }
                pawn.GetPack().name = curPackName;
                Find.WindowStack.TryRemove(this, true);
            }
        }
    }
}