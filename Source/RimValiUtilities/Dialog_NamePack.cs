using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{

    public class Dialog_NamePack : Window
    {

        private string curPackName;
        private Pawn pawn;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 175f);
            }
        }
        public Dialog_NamePack(Pawn pawn)
        {
            this.pawn = pawn;
            this.curPackName = pawn.GetPack().name;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
            this.closeOnAccept = false;
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
<<<<<<< HEAD
            string text = "PackNameLabel".Translate() +": ";
=======
            string text = "PackNameLabel".Translate(pawn.GetPack().name.Named("PACKNAME"));
>>>>>>> beta
            Widgets.Label(new Rect(15f, 15f, 500f, 50f), text);
            Text.Font = GameFont.Small;
            string text2 = Widgets.TextField(new Rect(15f, 50f, inRect.width / 2f - 20f, 35f), this.curPackName);
            if (text2.Length < 30 && CharacterCardUtility.ValidNameRegex.IsMatch(text2))
            {
                this.curPackName = text2;
            }
            if (Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "OK", true, true, true) || flag)
            {
                if (string.IsNullOrEmpty(this.curPackName))
                {
                    this.curPackName = pawn.GetPack().name;
                }
                pawn.GetPack().name = this.curPackName;
                Find.WindowStack.TryRemove(this, true);
            }
        }
    }
}