using System;
using System.Collections.Generic;
using System.Linq;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class HomeScreen : Widget
    {
        public int SelectedIndex = -1;
        int RecentCapacity = 0;

        HomeScreenButton NewProjectButton;
        HomeScreenButton OpenProjectButton;
        HomeScreenButton TutorialsButton;

        PictureBox YoutubeButton;
        PictureBox TwitterButton;

        MultilineLabel NoProjects;

        public HomeScreen(object Parent, string Name = "homeScreen")
            : base(Parent, Name)
        {
            SetBackgroundColor(28, 50, 73);
            Sprites["map"] = new Sprite(this.Viewport);
            if (System.IO.File.Exists("home_map.png")) Sprites["map"].Bitmap = new Bitmap("home_map.png");
            Sprites["sidebar"] = new Sprite(this.Viewport, "home_side.png");
            Sprites["logo"] = new Sprite(this.Viewport, "home_logo.png");
            Sprites["logo"].X = 33;
            Sprites["logo"].Y = 4;
            Sprites["text"] = new Sprite(this.Viewport, new Bitmap(360, 160));
            Sprites["text"].Bitmap.Font = Font.Get("Fonts/Ubuntu-R", 18); ;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.DrawText(Editor.GetVersionString(), 348, 88, Color.WHITE, DrawOptions.RightAlign);
            Sprites["text"].Bitmap.Font = Font.Get("Fonts/Ubuntu-B", 22);
            Sprites["text"].Bitmap.DrawText("Recent Projects:", 38, 126, Color.WHITE, DrawOptions.Underlined);
            Sprites["text"].Bitmap.Lock();
            Sprites["filesel"] = new Sprite(this.Viewport, new SolidBitmap(2, 38, new Color(0, 205, 255)));
            Sprites["filesel"].X = 30;
            Sprites["filesel"].Visible = false;
            Sprites["files"] = new Sprite(this.Viewport);
            Sprites["files"].X = 42;
            Sprites["files"].Y = 168;

            Bitmap b = new Bitmap(21, 21);
            #region Corner
            b.Unlock();
            b.SetPixel(20, 20, 0, 0, 0, 2);
            b.SetPixel(20, 19, 0, 0, 0, 3);
            b.SetPixel(19, 19, 0, 0, 0, 4);
            b.SetPixel(18, 20, 0, 0, 0, 4);
            b.SetPixel(20, 18, 0, 0, 0, 4);
            b.SetPixel(19, 18, 0, 0, 0, 5);
            b.SetPixel(18, 19, 0, 0, 0, 5);
            b.SetPixel(18, 18, 0, 0, 0, 6);
            b.SetPixel(17, 20, 0, 0, 0, 7);
            b.SetPixel(20, 17, 0, 0, 0, 7);
            b.SetPixel(17, 19, 0, 0, 0, 8);
            b.SetPixel(19, 17, 0, 0, 0, 8);
            b.SetPixel(17, 18, 0, 0, 0, 9);
            b.SetPixel(18, 17, 0, 0, 0, 9);
            b.SetPixel(16, 20, 0, 0, 0, 10);
            b.SetPixel(20, 16, 0, 0, 0, 10);
            b.SetPixel(16, 19, 0, 0, 0, 11);
            b.SetPixel(19, 16, 0, 0, 0, 11);
            b.SetPixel(16, 18, 0, 0, 0, 12);
            b.SetPixel(18, 16, 0, 0, 0, 12);
            b.SetPixel(17, 17, 0, 0, 0, 12);
            b.SetPixel(16, 17, 0, 0, 0, 15);
            b.SetPixel(17, 16, 0, 0, 0, 15);
            b.SetPixel(15, 20, 0, 0, 0, 16);
            b.SetPixel(20, 15, 0, 0, 0, 16);
            b.SetPixel(19, 15, 0, 0, 0, 17);
            b.SetPixel(15, 19, 0, 0, 0, 17);
            b.SetPixel(15, 18, 0, 0, 0, 18);
            b.SetPixel(18, 15, 0, 0, 0, 18);
            b.SetPixel(16, 16, 0, 0, 0, 18);
            b.SetPixel(15, 17, 0, 0, 0, 20);
            b.SetPixel(17, 15, 0, 0, 0, 20);
            b.SetPixel(14, 20, 0, 0, 0, 23);
            b.SetPixel(20, 14, 0, 0, 0, 23);
            b.SetPixel(15, 16, 0, 0, 0, 23);
            b.SetPixel(16, 15, 0, 0, 0, 24);
            b.SetPixel(14, 19, 0, 0, 0, 24);
            b.SetPixel(19, 14, 0, 0, 0, 24);
            b.SetPixel(14, 18, 0, 0, 0, 25);
            b.SetPixel(18, 14, 0, 0, 0, 25);
            b.SetPixel(17, 14, 0, 0, 0, 27);
            b.SetPixel(14, 17, 0, 0, 0, 28);
            b.SetPixel(15, 15, 0, 0, 0, 29);
            b.SetPixel(14, 16, 0, 0, 0, 30);
            b.SetPixel(16, 14, 0, 0, 0, 31);
            b.SetPixel(13, 20, 0, 0, 0, 33);
            b.SetPixel(20, 13, 0, 0, 0, 33);
            b.SetPixel(19, 13, 0, 0, 0, 34);
            b.SetPixel(13, 19, 0, 0, 0, 34);
            b.SetPixel(19, 13, 0, 0, 0, 34);
            b.SetPixel(13, 18, 0, 0, 0, 34);
            b.SetPixel(18, 13, 0, 0, 0, 35);
            b.SetPixel(15, 14, 0, 0, 0, 35);
            b.SetPixel(14, 15, 0, 0, 0, 35);
            b.SetPixel(13, 17, 0, 0, 0, 37);
            b.SetPixel(17, 13, 0, 0, 0, 37);
            b.SetPixel(13, 16, 0, 0, 0, 40);
            b.SetPixel(16, 13, 0, 0, 0, 40);
            b.SetPixel(14, 14, 0, 0, 0, 42);
            b.SetPixel(13, 15, 0, 0, 0, 45);
            b.SetPixel(15, 13, 0, 0, 0, 45);
            b.SetPixel(12, 20, 0, 0, 0, 45);
            b.SetPixel(20, 12, 0, 0, 0, 45);
            b.SetPixel(12, 19, 0, 0, 0, 46);
            b.SetPixel(19, 12, 0, 0, 0, 46);
            b.SetPixel(12, 18, 0, 0, 0, 47);
            b.SetPixel(18, 12, 0, 0, 0, 47);
            b.SetPixel(12, 17, 0, 0, 0, 49);
            b.SetPixel(17, 12, 0, 0, 0, 49);
            b.SetPixel(13, 14, 0, 0, 0, 51);
            b.SetPixel(14, 13, 0, 0, 0, 51);
            b.SetPixel(12, 16, 0, 0, 0, 52);
            b.SetPixel(16, 12, 0, 0, 0, 52);
            b.SetPixel(15, 12, 0, 0, 0, 56);
            b.SetPixel(12, 15, 0, 0, 0, 57);
            b.SetPixel(13, 13, 0, 0, 0, 60);
            b.SetPixel(11, 20, 0, 0, 0, 60);
            b.SetPixel(20, 11, 0, 0, 0, 60);
            b.SetPixel(19, 11, 0, 0, 0, 60);
            b.SetPixel(11, 19, 0, 0, 0, 61);
            b.SetPixel(11, 18, 0, 0, 0, 61);
            b.SetPixel(18, 11, 0, 0, 0, 62);
            b.SetPixel(12, 14, 0, 0, 0, 62);
            b.SetPixel(14, 12, 0, 0, 0, 62);
            b.SetPixel(17, 11, 0, 0, 0, 63);
            b.SetPixel(11, 17, 0, 0, 0, 64);
            b.SetPixel(16, 11, 0, 0, 0, 66);
            b.SetPixel(11, 16, 0, 0, 0, 66);
            b.SetPixel(15, 11, 0, 0, 0, 70);
            b.SetPixel(13, 12, 0, 0, 0, 70);
            b.SetPixel(12, 13, 0, 0, 0, 71);
            b.SetPixel(11, 15, 0, 0, 0, 71);
            b.SetPixel(14, 11, 0, 0, 0, 76);
            b.SetPixel(11, 14, 0, 0, 0, 76);
            b.SetPixel(10, 20, 0, 0, 0, 77);
            b.SetPixel(20, 10, 0, 0, 0, 78);
            b.SetPixel(10, 19, 0, 0, 0, 78);
            b.SetPixel(19, 10, 0, 0, 0, 78);
            b.SetPixel(10, 18, 0, 0, 0, 79);
            b.SetPixel(18, 10, 0, 0, 0, 79);
            b.SetPixel(12, 12, 0, 0, 0, 81);
            b.SetPixel(17, 10, 0, 0, 0, 81);
            b.SetPixel(10, 17, 0, 0, 0, 81);
            b.SetPixel(13, 11, 0, 0, 0, 83);
            b.SetPixel(10, 16, 0, 0, 0, 83);
            b.SetPixel(11, 13, 0, 0, 0, 84);
            b.SetPixel(16, 10, 0, 0, 0, 84);
            b.SetPixel(15, 10, 0, 0, 0, 87);
            b.SetPixel(10, 15, 0, 0, 0, 87);
            b.SetPixel(14, 10, 0, 0, 0, 92);
            b.SetPixel(10, 14, 0, 0, 0, 92);
            b.SetPixel(12, 11, 0, 0, 0, 93);
            b.SetPixel(11, 12, 0, 0, 0, 93);
            b.SetPixel(20, 9, 0, 0, 0, 97);
            b.SetPixel(9, 20, 0, 0, 0, 97);
            b.SetPixel(19, 9, 0, 0, 0, 97);
            b.SetPixel(9, 19, 0, 0, 0, 97);
            b.SetPixel(18, 9, 0, 0, 0, 98);
            b.SetPixel(9, 18, 0, 0, 0, 98);
            b.SetPixel(13, 10, 0, 0, 0, 99);
            b.SetPixel(10, 13, 0, 0, 0, 99);
            b.SetPixel(17, 9, 0, 0, 0, 100);
            b.SetPixel(9, 17, 0, 0, 0, 100);
            b.SetPixel(16, 9, 0, 0, 0, 102);
            b.SetPixel(9, 16, 0, 0, 0, 102);
            b.SetPixel(11, 11, 0, 0, 0, 104);
            b.SetPixel(15, 9, 0, 0, 0, 105);
            b.SetPixel(9, 15, 0, 0, 0, 105);
            b.SetPixel(12, 10, 0, 0, 0, 108);
            b.SetPixel(10, 12, 0, 0, 0, 107);
            b.SetPixel(14, 9, 0, 0, 0, 110);
            b.SetPixel(9, 14, 0, 0, 0, 110);
            b.SetPixel(13, 9, 0, 0, 0, 116);
            b.SetPixel(9, 13, 0, 0, 0, 116);
            b.SetPixel(20, 8, 0, 0, 0, 117);
            b.SetPixel(8, 20, 0, 0, 0, 117);
            b.SetPixel(19, 8, 0, 0, 0, 118);
            b.SetPixel(8, 19, 0, 0, 0, 118);
            b.SetPixel(11, 10, 0, 0, 0, 118);
            b.SetPixel(10, 11, 0, 0, 0, 118);
            b.SetPixel(18, 8, 0, 0, 0, 119);
            b.SetPixel(8, 18, 0, 0, 0, 119);
            b.SetPixel(17, 8, 0, 0, 0, 120);
            b.SetPixel(8, 17, 0, 0, 0, 120);
            b.SetPixel(16, 8, 0, 0, 0, 122);
            b.SetPixel(8, 16, 0, 0, 0, 122);
            b.SetPixel(12, 9, 0, 0, 0, 124);
            b.SetPixel(9, 12, 0, 0, 0, 124);
            b.SetPixel(15, 8, 0, 0, 0, 125);
            b.SetPixel(8, 15, 0, 0, 0, 125);
            b.SetPixel(14, 8, 0, 0, 0, 129);
            b.SetPixel(8, 14, 0, 0, 0, 129);
            b.SetPixel(10, 10, 0, 0, 0, 130);
            b.SetPixel(11, 9, 0, 0, 0, 133);
            b.SetPixel(9, 11, 0, 0, 0, 133);
            b.SetPixel(13, 8, 0, 0, 0, 134);
            b.SetPixel(8, 13, 0, 0, 0, 134);
            b.SetPixel(20, 7, 0, 0, 0, 138);
            b.SetPixel(7, 20, 0, 0, 0, 139);
            b.SetPixel(19, 7, 0, 0, 0, 139);
            b.SetPixel(7, 19, 0, 0, 0, 139);
            b.SetPixel(18, 7, 0, 0, 0, 140);
            b.SetPixel(7, 18, 0, 0, 0, 140);
            b.SetPixel(17, 7, 0, 0, 0, 141);
            b.SetPixel(7, 17, 0, 0, 0, 141);
            b.SetPixel(8, 12, 0, 0, 0, 141);
            b.SetPixel(12, 8, 0, 0, 0, 141);
            b.SetPixel(16, 7, 0, 0, 0, 142);
            b.SetPixel(7, 16, 0, 0, 0, 142);
            b.SetPixel(10, 9, 0, 0, 0, 144);
            b.SetPixel(9, 10, 0, 0, 0, 144);
            b.SetPixel(15, 7, 0, 0, 0, 145);
            b.SetPixel(7, 15, 0, 0, 0, 145);
            b.SetPixel(14, 7, 0, 0, 0, 148);
            b.SetPixel(7, 14, 0, 0, 0, 148);
            b.SetPixel(11, 8, 0, 0, 0, 149);
            b.SetPixel(8, 11, 0, 0, 0, 149);
            b.SetPixel(13, 7, 0, 0, 0, 153);
            b.SetPixel(7, 13, 0, 0, 0, 153);
            b.SetPixel(9, 9, 0, 0, 0, 156);
            b.SetPixel(12, 7, 0, 0, 0, 158);
            b.SetPixel(7, 12, 0, 0, 0, 158);
            b.SetPixel(10, 8, 0, 0, 0, 158);
            b.SetPixel(8, 10, 0, 0, 0, 159);
            b.SetPixel(6, 20, 0, 0, 0, 159);
            b.SetPixel(20, 6, 0, 0, 0, 159);
            b.SetPixel(6, 20, 0, 0, 0, 159);
            b.SetPixel(20, 6, 0, 0, 0, 159);
            b.SetPixel(19, 6, 0, 0, 0, 160);
            b.SetPixel(6, 19, 0, 0, 0, 160);
            b.SetPixel(18, 6, 0, 0, 0, 160);
            b.SetPixel(6, 18, 0, 0, 0, 160);
            b.SetPixel(17, 6, 0, 0, 0, 161);
            b.SetPixel(6, 17, 0, 0, 0, 161);
            b.SetPixel(6, 16, 0, 0, 0, 162);
            b.SetPixel(16, 6, 0, 0, 0, 163);
            b.SetPixel(15, 6, 0, 0, 0, 165);
            b.SetPixel(6, 15, 0, 0, 0, 165);
            b.SetPixel(11, 7, 0, 0, 0, 165);
            b.SetPixel(7, 11, 0, 0, 0, 165);
            b.SetPixel(6, 14, 0, 0, 0, 167);
            b.SetPixel(14, 6, 0, 0, 0, 167);
            b.SetPixel(9, 8, 0, 0, 0, 169);
            b.SetPixel(8, 9, 0, 0, 0, 169);
            b.SetPixel(13, 6, 0, 0, 0, 171);
            b.SetPixel(6, 13, 0, 0, 0, 171);
            b.SetPixel(10, 7, 0, 0, 0, 173);
            b.SetPixel(7, 10, 0, 0, 0, 173);
            b.SetPixel(0, 10, 0, 0, 0, 173);
            b.SetPixel(12, 6, 0, 0, 0, 176);
            b.SetPixel(6, 12, 0, 0, 0, 176);
            b.SetPixel(20, 5, 0, 0, 0, 178);
            b.SetPixel(5, 20, 0, 0, 0, 179);
            b.SetPixel(19, 5, 0, 0, 0, 179);
            b.SetPixel(5, 19, 0, 0, 0, 179);
            b.SetPixel(18, 5, 0, 0, 0, 179);
            b.SetPixel(5, 18, 0, 0, 0, 179);
            b.SetPixel(17, 5, 0, 0, 0, 180);
            b.SetPixel(5, 17, 0, 0, 0, 180);
            b.SetPixel(8, 8, 0, 0, 0, 180);
            b.SetPixel(16, 5, 0, 0, 0, 181);
            b.SetPixel(5, 16, 0, 0, 0, 181);
            b.SetPixel(11, 6, 0, 0, 0, 181);
            b.SetPixel(6, 11, 0, 0, 0, 181);
            b.SetPixel(15, 5, 0, 0, 0, 182);
            b.SetPixel(9, 7, 0, 0, 0, 182);
            b.SetPixel(7, 9, 0, 0, 0, 182);
            b.SetPixel(5, 15, 0, 0, 0, 183);
            b.SetPixel(14, 5, 0, 0, 0, 185);
            b.SetPixel(5, 14, 0, 0, 0, 185);
            b.SetPixel(13, 5, 0, 0, 0, 188);
            b.SetPixel(5, 13, 0, 0, 0, 188);
            b.SetPixel(10, 6, 0, 0, 0, 188);
            b.SetPixel(6, 10, 0, 0, 0, 188);
            b.SetPixel(12, 5, 0, 0, 0, 191);
            b.SetPixel(5, 12, 0, 0, 0, 192);
            b.SetPixel(7, 8, 0, 0, 0, 192);
            b.SetPixel(8, 7, 0, 0, 0, 192);
            b.SetPixel(9, 6, 0, 0, 0, 195);
            b.SetPixel(6, 9, 0, 0, 0, 195);
            b.SetPixel(11, 5, 0, 0, 0, 196);
            b.SetPixel(5, 11, 0, 0, 0, 196);
            b.SetPixel(20, 4, 0, 0, 0, 196);
            b.SetPixel(4, 20, 0, 0, 0, 196);
            b.SetPixel(19, 4, 0, 0, 0, 196);
            b.SetPixel(4, 19, 0, 0, 0, 196);
            b.SetPixel(18, 4, 0, 0, 0, 197);
            b.SetPixel(4, 18, 0, 0, 0, 197);
            b.SetPixel(17, 4, 0, 0, 0, 197);
            b.SetPixel(4, 17, 0, 0, 0, 197);
            b.SetPixel(16, 4, 0, 0, 0, 198);
            b.SetPixel(4, 16, 0, 0, 0, 198);
            b.SetPixel(15, 4, 0, 0, 0, 199);
            b.SetPixel(4, 15, 0, 0, 0, 199);
            b.SetPixel(14, 4, 0, 0, 0, 201);
            b.SetPixel(4, 14, 0, 0, 0, 201);
            b.SetPixel(7, 7, 0, 0, 0, 201);
            b.SetPixel(10, 5, 0, 0, 0, 201);
            b.SetPixel(5, 10, 0, 0, 0, 202);
            b.SetPixel(6, 8, 0, 0, 0, 203);
            b.SetPixel(8, 6, 0, 0, 0, 203);
            b.SetPixel(13, 4, 0, 0, 0, 203);
            b.SetPixel(4, 13, 0, 0, 0, 203);
            b.SetPixel(12, 4, 0, 0, 0, 206);
            b.SetPixel(4, 12, 0, 0, 0, 206);
            b.SetPixel(9, 5, 0, 0, 0, 207);
            b.SetPixel(5, 9, 0, 0, 0, 207);
            b.SetPixel(11, 4, 0, 0, 0, 210);
            b.SetPixel(4, 11, 0, 0, 0, 210);
            b.SetPixel(7, 6, 0, 0, 0, 211);
            b.SetPixel(6, 7, 0, 0, 0, 211);
            b.SetPixel(8, 5, 0, 0, 0, 213);
            b.SetPixel(5, 8, 0, 0, 0, 214);
            b.SetPixel(10, 4, 0, 0, 0, 214);
            b.SetPixel(4, 10, 0, 0, 0, 214);
            b.SetPixel(20, 3, 0, 0, 0, 211);
            b.SetPixel(3, 20, 0, 0, 0, 211);
            b.SetPixel(19, 3, 0, 0, 0, 211);
            b.SetPixel(3, 19, 0, 0, 0, 211);
            b.SetPixel(3, 18, 0, 0, 0, 211);
            b.SetPixel(18, 3, 0, 0, 0, 212);
            b.SetPixel(17, 3, 0, 0, 0, 212);
            b.SetPixel(3, 17, 0, 0, 0, 212);
            b.SetPixel(16, 3, 0, 0, 0, 213);
            b.SetPixel(3, 16, 0, 0, 0, 213);
            b.SetPixel(15, 3, 0, 0, 0, 214);
            b.SetPixel(3, 15, 0, 0, 0, 214);
            b.SetPixel(14, 3, 0, 0, 0, 215);
            b.SetPixel(3, 14, 0, 0, 0, 215);
            b.SetPixel(13, 3, 0, 0, 0, 216);
            b.SetPixel(3, 13, 0, 0, 0, 216);
            b.SetPixel(9, 4, 0, 0, 0, 218);
            b.SetPixel(4, 9, 0, 0, 0, 218);
            b.SetPixel(12, 3, 0, 0, 0, 219);
            b.SetPixel(3, 12, 0, 0, 0, 219);
            b.SetPixel(6, 6, 0, 0, 0, 219);
            b.SetPixel(7, 5, 0, 0, 0, 220);
            b.SetPixel(5, 7, 0, 0, 0, 220);
            b.SetPixel(11, 3, 0, 0, 0, 221);
            b.SetPixel(3, 11, 0, 0, 0, 221);
            b.SetPixel(8, 4, 0, 0, 0, 223);
            b.SetPixel(4, 8, 0, 0, 0, 223);
            b.SetPixel(20, 2, 0, 0, 0, 223);
            b.SetPixel(2, 20, 0, 0, 0, 223);
            b.SetPixel(19, 2, 0, 0, 0, 223);
            b.SetPixel(18, 2, 0, 0, 0, 223);
            b.SetPixel(2, 19, 0, 0, 0, 224);
            b.SetPixel(2, 18, 0, 0, 0, 224);
            b.SetPixel(17, 2, 0, 0, 0, 224);
            b.SetPixel(2, 17, 0, 0, 0, 224);
            b.SetPixel(16, 2, 0, 0, 0, 224);
            b.SetPixel(2, 16, 0, 0, 0, 224);
            b.SetPixel(10, 3, 0, 0, 0, 224);
            b.SetPixel(3, 10, 0, 0, 0, 224);
            b.SetPixel(15, 2, 0, 0, 0, 225);
            b.SetPixel(2, 15, 0, 0, 0, 225);
            b.SetPixel(14, 2, 0, 0, 0, 226);
            b.SetPixel(2, 14, 0, 0, 0, 226);
            b.SetPixel(6, 5, 0, 0, 0, 226);
            b.SetPixel(5, 6, 0, 0, 0, 226);
            b.SetPixel(13, 2, 0, 0, 0, 227);
            b.SetPixel(2, 13, 0, 0, 0, 227);
            b.SetPixel(7, 4, 0, 0, 0, 228);
            b.SetPixel(4, 7, 0, 0, 0, 228);
            b.SetPixel(9, 3, 0, 0, 0, 228);
            b.SetPixel(3, 9, 0, 0, 0, 228);
            b.SetPixel(12, 2, 0, 0, 0, 229);
            b.SetPixel(2, 12, 0, 0, 0, 229);
            b.SetPixel(11, 2, 0, 0, 0, 230);
            b.SetPixel(2, 11, 0, 0, 0, 231);
            b.SetPixel(8, 3, 0, 0, 0, 231);
            b.SetPixel(3, 8, 0, 0, 0, 231);
            b.SetPixel(5, 5, 0, 0, 0, 232);
            b.SetPixel(6, 4, 0, 0, 0, 232);
            b.SetPixel(4, 6, 0, 0, 0, 232);
            b.SetPixel(10, 2, 0, 0, 0, 233);
            b.SetPixel(2, 10, 0, 0, 0, 233);
            b.SetPixel(20, 1, 0, 0, 0, 233);
            b.SetPixel(1, 20, 0, 0, 0, 233);
            b.SetPixel(19, 1, 0, 0, 0, 233);
            b.SetPixel(1, 19, 0, 0, 0, 233);
            b.SetPixel(18, 1, 0, 0, 0, 233);
            b.SetPixel(1, 18, 0, 0, 0, 233);
            b.SetPixel(17, 1, 0, 0, 0, 233);
            b.SetPixel(1, 17, 0, 0, 0, 234);
            b.SetPixel(16, 1, 0, 0, 0, 234);
            b.SetPixel(1, 16, 0, 0, 0, 234);
            b.SetPixel(15, 1, 0, 0, 0, 234);
            b.SetPixel(1, 15, 0, 0, 0, 234);
            b.SetPixel(14, 1, 0, 0, 0, 235);
            b.SetPixel(1, 14, 0, 0, 0, 235);
            b.SetPixel(9, 2, 0, 0, 0, 235);
            b.SetPixel(2, 9, 0, 0, 0, 235);
            b.SetPixel(7, 3, 0, 0, 0, 235);
            b.SetPixel(3, 7, 0, 0, 0, 235);
            b.SetPixel(13, 1, 0, 0, 0, 236);
            b.SetPixel(1, 13, 0, 0, 0, 236);
            b.SetPixel(12, 1, 0, 0, 0, 237);
            b.SetPixel(1, 12, 0, 0, 0, 237);
            b.SetPixel(5, 4, 0, 0, 0, 237);
            b.SetPixel(4, 5, 0, 0, 0, 237);
            b.SetPixel(11, 1, 0, 0, 0, 238);
            b.SetPixel(1, 11, 0, 0, 0, 238);
            b.SetPixel(8, 2, 0, 0, 0, 238);
            b.SetPixel(2, 8, 0, 0, 0, 238);
            b.SetPixel(6, 3, 0, 0, 0, 238);
            b.SetPixel(3, 6, 0, 0, 0, 238);
            b.SetPixel(10, 1, 0, 0, 0, 240);
            b.SetPixel(1, 10, 0, 0, 0, 240);
            b.SetPixel(7, 2, 0, 0, 0, 240);
            b.SetPixel(2, 7, 0, 0, 0, 240);
            b.SetPixel(20, 0, 0, 0, 0, 240);
            b.SetPixel(0, 20, 0, 0, 0, 240);
            b.SetPixel(19, 0, 0, 0, 0, 240);
            b.SetPixel(0, 19, 0, 0, 0, 240);
            b.SetPixel(18, 0, 0, 0, 0, 240);
            b.SetPixel(17, 0, 0, 0, 0, 240);
            b.SetPixel(0, 18, 0, 0, 0, 241);
            b.SetPixel(0, 17, 0, 0, 0, 241);
            b.SetPixel(9, 1, 0, 0, 0, 241);
            b.SetPixel(1, 9, 0, 0, 0, 241);
            b.SetPixel(16, 0, 0, 0, 0, 241);
            b.SetPixel(0, 16, 0, 0, 0, 241);
            b.SetPixel(15, 0, 0, 0, 0, 241);
            b.SetPixel(0, 15, 0, 0, 0, 241);
            b.SetPixel(14, 0, 0, 0, 0, 241);
            b.SetPixel(4, 4, 0, 0, 0, 241);
            b.SetPixel(0, 14, 0, 0, 0, 242);
            b.SetPixel(13, 0, 0, 0, 0, 242);
            b.SetPixel(0, 13, 0, 0, 0, 242);
            b.SetPixel(5, 3, 0, 0, 0, 242);
            b.SetPixel(3, 5, 0, 0, 0, 242);
            b.SetPixel(6, 2, 0, 0, 0, 243);
            b.SetPixel(2, 6, 0, 0, 0, 243);
            b.SetPixel(8, 1, 0, 0, 0, 243);
            b.SetPixel(1, 8, 0, 0, 0, 243);
            b.SetPixel(12, 0, 0, 0, 0, 243);
            b.SetPixel(0, 12, 0, 0, 0, 243);
            b.SetPixel(11, 0, 0, 0, 0, 243);
            b.SetPixel(0, 11, 0, 0, 0, 244);
            b.SetPixel(10, 0, 0, 0, 0, 245);
            b.SetPixel(0, 10, 0, 0, 0, 245);
            b.SetPixel(7, 1, 0, 0, 0, 245);
            b.SetPixel(1, 7, 0, 0, 0, 245);
            b.SetPixel(5, 2, 0, 0, 0, 245);
            b.SetPixel(2, 5, 0, 0, 0, 245);
            b.SetPixel(4, 3, 0, 0, 0, 245);
            b.SetPixel(3, 4, 0, 0, 0, 245);
            b.SetPixel(9, 0, 0, 0, 0, 246);
            b.SetPixel(0, 9, 0, 0, 0, 246);
            b.SetPixel(8, 0, 0, 0, 0, 247);
            b.SetPixel(0, 8, 0, 0, 0, 247);
            b.SetPixel(6, 1, 0, 0, 0, 247);
            b.SetPixel(1, 6, 0, 0, 0, 247);
            b.SetPixel(3, 3, 0, 0, 0, 247);
            b.SetPixel(7, 0, 0, 0, 0, 248);
            b.SetPixel(0, 7, 0, 0, 0, 248);
            b.SetPixel(5, 1, 0, 0, 0, 248);
            b.SetPixel(1, 5, 0, 0, 0, 248);
            b.SetPixel(4, 2, 0, 0, 0, 248);
            b.SetPixel(2, 4, 0, 0, 0, 248);
            b.SetPixel(6, 0, 0, 0, 0, 249);
            b.SetPixel(0, 6, 0, 0, 0, 249);
            b.SetPixel(3, 2, 0, 0, 0, 249);
            b.SetPixel(2, 3, 0, 0, 0, 250);
            b.SetPixel(5, 0, 0, 0, 0, 250);
            b.SetPixel(4, 1, 0, 0, 0, 250);
            b.SetPixel(1, 4, 0, 0, 0, 250);
            b.SetPixel(0, 5, 0, 0, 0, 251);
            b.SetPixel(3, 1, 0, 0, 0, 251);
            b.SetPixel(1, 3, 0, 0, 0, 251);
            b.SetPixel(2, 2, 0, 0, 0, 251);
            b.SetPixel(4, 0, 0, 0, 0, 252);
            b.SetPixel(0, 4, 0, 0, 0, 252);
            b.SetPixel(3, 0, 0, 0, 0, 252);
            b.SetPixel(0, 3, 0, 0, 0, 252);
            b.SetPixel(2, 1, 0, 0, 0, 252);
            b.SetPixel(1, 2, 0, 0, 0, 252);
            b.SetPixel(2, 0, 0, 0, 0, 253);
            b.SetPixel(0, 2, 0, 0, 0, 253);
            b.SetPixel(1, 1, 0, 0, 0, 253);
            b.SetPixel(1, 0, 0, 0, 0, 254);
            b.SetPixel(0, 1, 0, 0, 0, 254);
            b.SetPixel(0, 0, 0, 0, 0, 255);
            b.Lock();
            #endregion
            Sprites["topleft"] = new Sprite(this.Viewport, b);
            Sprites["bottomleft"] = new Sprite(this.Viewport, b);
            Sprites["bottomleft"].MirrorY = true;
            Sprites["topright"] = new Sprite(this.Viewport, b);
            Sprites["topright"].MirrorX = true;
            Sprites["bottomright"] = new Sprite(this.Viewport, b);
            Sprites["bottomright"].MirrorX = Sprites["bottomright"].MirrorY = true;

            Bitmap h = new Bitmap(21, 1);
            #region Horizontal Side
            h.Unlock();
            h.SetPixel(20, 0, 0, 0, 0, 1);
            h.SetPixel(19, 0, 0, 0, 0, 2);
            h.SetPixel(18, 0, 0, 0, 0, 3);
            h.SetPixel(17, 0, 0, 0, 0, 6);
            h.SetPixel(16, 0, 0, 0, 0, 9);
            h.SetPixel(15, 0, 0, 0, 0, 15);
            h.SetPixel(14, 0, 0, 0, 0, 22);
            h.SetPixel(13, 0, 0, 0, 0, 32);
            h.SetPixel(12, 0, 0, 0, 0, 44);
            h.SetPixel(11, 0, 0, 0, 0, 59);
            h.SetPixel(10, 0, 0, 0, 0, 77);
            h.SetPixel(9, 0, 0, 0, 0, 96);
            h.SetPixel(8, 0, 0, 0, 0, 117);
            h.SetPixel(7, 0, 0, 0, 0, 138);
            h.SetPixel(6, 0, 0, 0, 0, 159);
            h.SetPixel(5, 0, 0, 0, 0, 178);
            h.SetPixel(4, 0, 0, 0, 0, 196);
            h.SetPixel(3, 0, 0, 0, 0, 211);
            h.SetPixel(2, 0, 0, 0, 0, 223);
            h.SetPixel(1, 0, 0, 0, 0, 233);
            h.SetPixel(0, 0, 0, 0, 0, 240);
            h.Lock();
            #endregion

            Sprites["left"] = new Sprite(this.Viewport, h);
            Sprites["left"].Y = 21;
            Sprites["right"] = new Sprite(this.Viewport, h);
            Sprites["right"].Y = Sprites["left"].Y;
            Sprites["right"].MirrorX = true;

            Bitmap v = new Bitmap(1, 21);
            #region Vertical Side
            v.Unlock();
            v.SetPixel(0, 20, 0, 0, 0, 1);
            v.SetPixel(0, 19, 0, 0, 0, 2);
            v.SetPixel(0, 18, 0, 0, 0, 3);
            v.SetPixel(0, 17, 0, 0, 0, 6);
            v.SetPixel(0, 16, 0, 0, 0, 9);
            v.SetPixel(0, 15, 0, 0, 0, 15);
            v.SetPixel(0, 14, 0, 0, 0, 22);
            v.SetPixel(0, 13, 0, 0, 0, 32);
            v.SetPixel(0, 12, 0, 0, 0, 44);
            v.SetPixel(0, 11, 0, 0, 0, 59);
            v.SetPixel(0, 10, 0, 0, 0, 77);
            v.SetPixel(0, 9, 0, 0, 0, 96);
            v.SetPixel(0, 8, 0, 0, 0, 117);
            v.SetPixel(0, 7, 0, 0, 0, 138);
            v.SetPixel(0, 6, 0, 0, 0, 159);
            v.SetPixel(0, 5, 0, 0, 0, 178);
            v.SetPixel(0, 4, 0, 0, 0, 196);
            v.SetPixel(0, 3, 0, 0, 0, 211);
            v.SetPixel(0, 2, 0, 0, 0, 223);
            v.SetPixel(0, 1, 0, 0, 0, 233);
            v.SetPixel(0, 0, 0, 0, 0, 240);
            v.Lock();
            #endregion

            Sprites["top"] = new Sprite(this.Viewport, v);
            Sprites["top"].X = 21;
            Sprites["bottom"] = new Sprite(this.Viewport, v);
            Sprites["bottom"].X = Sprites["top"].X;
            Sprites["bottom"].MirrorY = true;

            NewProjectButton = new HomeScreenButton(this);
            NewProjectButton.SetPosition(445, 108);
            NewProjectButton.SetText("New Project");
            NewProjectButton.SetIcon("home_icon_new");
            NewProjectButton.SetHelpText("Create a new project.");
            NewProjectButton.WidgetIM.OnLeftClick += delegate (object sender, MouseEventArgs e)
            {
                NewProject();
            };

            OpenProjectButton = new HomeScreenButton(this);
            OpenProjectButton.SetPosition(690, 108);
            OpenProjectButton.SetText("Open Project");
            OpenProjectButton.SetIcon("home_icon_openfile");
            OpenProjectButton.SetHelpText("Open an existing project by selecting its project file.");
            OpenProjectButton.WidgetIM.OnLeftClick += delegate (object sender, MouseEventArgs e)
            {
                OpenProject();
            };

            TutorialsButton = new HomeScreenButton(this);
            TutorialsButton.SetPosition(935, 108);
            TutorialsButton.SetText("Tutorials");
            TutorialsButton.SetHelpText("Click this button to be directed to various tutorials and documentation for RPG Studio MK.");
            TutorialsButton.SetIcon("home_icon_tutorials");
            TutorialsButton.WidgetIM.OnLeftClick += delegate (object sender, MouseEventArgs e)
            {
                ShowTutorials();
            };

            YoutubeButton = new PictureBox(this);
            YoutubeButton.Sprite.Bitmap = new Bitmap("home_icon_youtube.png");
            YoutubeButton.SetHelpText("Visit MK's YouTube account.");
            YoutubeButton.WidgetIM.OnLeftClick += delegate (object sender, MouseEventArgs e)
            {
                new MessageBox("Oops!", "MK does not have a YouTube channel yet!");
            };
            
            TwitterButton = new PictureBox(this);
            TwitterButton.Sprite.Bitmap = new Bitmap("home_icon_twitter.png");
            TwitterButton.SetHelpText("Visit MK's Twitter account.");
            TwitterButton.WidgetIM.OnLeftClick += delegate (object sender, MouseEventArgs e)
            {
                Utilities.OpenLink("http://twitter.com/MKStarterKit");
            };

            WidgetIM.OnMouseMoving += MouseMoving;
            WidgetIM.OnHoverChanged += HoverChanged;
            WidgetIM.OnMouseDown += MouseDown;

            NoProjects = new MultilineLabel(this);
            NoProjects.SetSize(320, 100);
            NoProjects.SetPosition(40, 170);
            NoProjects.SetText("You haven't opened any projects recently.\nGet started by creating or opening a project!");
            NoProjects.SetFont(Font.Get("Fonts/Ubuntu-R", 15));
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);

            #region Shadow
            Sprites["topright"].X = Size.Width - 21;
            Sprites["bottomleft"].Y = Size.Height - 21;
            Sprites["bottomright"].X = Sprites["topright"].X;
            Sprites["bottomright"].Y = Sprites["bottomleft"].Y;
            Sprites["right"].X = Sprites["topright"].X;
            Sprites["bottom"].Y = Sprites["bottomleft"].Y;
            Sprites["left"].ZoomY = Size.Height - 42;
            Sprites["top"].ZoomX = Size.Width - 42;
            Sprites["right"].ZoomY = Sprites["left"].ZoomY;
            Sprites["bottom"].ZoomX = Sprites["top"].ZoomX;
            #endregion

            #region Sidebar
            Sprites["sidebar"].SrcRect.Height = Sprites["sidebar"].Bitmap.Height;
            if (Size.Height <= Sprites["sidebar"].Bitmap.Height) Sprites["sidebar"].SrcRect.Height = Size.Height;
            else
            {
                double factor = Size.Height / (double) Sprites["sidebar"].Bitmap.Height;
                Sprites["sidebar"].ZoomY = factor;
            }
            #endregion

            #region Map
            if (Sprites["map"].Bitmap != null)
            {
                int dx = Size.Width - Sprites["map"].Bitmap.Width;
                int dy = Size.Height - Sprites["map"].Bitmap.Height;
                double px = dx / (double) Sprites["map"].Bitmap.Width;
                double py = dy / (double) Sprites["map"].Bitmap.Height;
                if (dx != 0 || dy != 0)
                {
                    // Zooms the sprite (maintaining aspect ratio) and centers it
                    if (px > py) Sprites["map"].ZoomX = Sprites["map"].ZoomY = 1 + px;
                    else Sprites["map"].ZoomX = Sprites["map"].ZoomY = 1 + py;
                    Sprites["map"].X = Size.Width / 2 - (int) Math.Round(Sprites["map"].Bitmap.Width * Sprites["map"].ZoomX / 2d);
                    Sprites["map"].Y = Size.Height / 2 - (int) Math.Round(Sprites["map"].Bitmap.Height * Sprites["map"].ZoomY / 2d);
                }
            }
            #endregion

            #region Recent Files
            if (Sprites["files"].Bitmap != null) Sprites["files"].Bitmap.Dispose();
            int height = Size.Height - 190;
            if (height < 1) return;
            RecentCapacity = (int) Math.Floor(height / 48d);
            Sprites["files"].Bitmap = new Bitmap(314, 48 * RecentCapacity);
            Sprites["files"].Bitmap.Unlock();
            Font boldfont = Font.Get("Fonts/Ubuntu-B", 16);
            Font regularfont = Font.Get("Fonts/Ubuntu-R", 14);
            for (int i = 0; i < Editor.GeneralSettings.RecentFiles.Count; i++)
            {
                if (i >= RecentCapacity) break;
                string name = Editor.GeneralSettings.RecentFiles[i][0];
                string projectpath = Editor.GeneralSettings.RecentFiles[i][1];
                while (projectpath.Contains("\\")) projectpath = projectpath.Replace("\\", "/");
                Sprites["files"].Bitmap.Font = boldfont;
                Sprites["files"].Bitmap.DrawText(name, 0, 48 * i + 4, Color.WHITE);
                Sprites["files"].Bitmap.Font = regularfont;
                List<string> folders = projectpath.Split('/').ToList();
                string path = "";
                for (int j = folders.Count - 2; j >= 0; j--)
                {
                    string add = "/";
                    if (folders[j].Contains(":")) add = "";
                    Size s = Sprites["files"].Bitmap.Font.TextSize(add + folders[j] + path);
                    if (s.Width > Sprites["files"].Bitmap.Width - 39)
                    {
                        path = "..." + path;
                        break;
                    }
                    else
                    {
                        path = add + folders[j] + path;
                    }
                }
                Sprites["files"].Bitmap.DrawText(path, 30, 48 * i + 22, Color.WHITE);
            }
            Sprites["files"].Bitmap.Lock();
            NoProjects.SetVisible(Editor.GeneralSettings.RecentFiles.Count == 0);
            #endregion

            #region Buttons
            int windowheight = Window.Height;

            bool Hor3 = true;
            bool CenterY = false;
            bool Hor2Ver1 = false;
            bool Ver3 = false;
            bool Invis = false;
            bool HorSM = true;
            bool VerSM = false;
            bool InvisSM = false;

            if (Size.Width < 1180)
            {
                Hor3 = false;
                Hor2Ver1 = true;
            }
            if (Size.Width < 930)
            {
                Hor2Ver1 = false;
                Ver3 = true;
                HorSM = false;
                VerSM = true;
            }
            if (Size.Width < 860)
            {
                VerSM = false;
                InvisSM = true;
            }
            if (Size.Width < 780)
            {
                Ver3 = false;
                Invis = true;
                InvisSM = false;
                VerSM = true;
            }

            if (Hor3 && HorSM && windowheight < 500)
            {
                HorSM = false;
                InvisSM = true;
                CenterY = true;
                if (Size.Width < 1210)
                {
                    Hor3 = false;
                    CenterY = false;
                    VerSM = true;
                    InvisSM = false;
                    Invis = true;
                }
                else if (Size.Width >= 1300)
                {
                    VerSM = true;
                    InvisSM = false;
                }
            }

            if (Hor2Ver1 && windowheight < 650)
            {
                Hor2Ver1 = false;
                Invis = true;
                HorSM = false;
                VerSM = true;
            }

            if (Ver3)
            {
                if (windowheight < 720)
                {
                    Ver3 = false;
                    Invis = true;
                    HorSM = false;
                    VerSM = true;
                }
                else if (windowheight >= 810)
                {
                    VerSM = false;
                    HorSM = true;
                    InvisSM = false;
                }
            }

            NewProjectButton.SetVisible(true);
            OpenProjectButton.SetVisible(true);
            TutorialsButton.SetVisible(true);
            if (Hor3)
            {
                int y = CenterY ? Size.Height / 2 - NewProjectButton.Size.Height / 2 : 108;
                int addx = CenterY ? 30 : 0;
                NewProjectButton.SetPosition(445 + addx, y);
                OpenProjectButton.SetPosition(690 + addx, y);
                TutorialsButton.SetPosition(935 + addx, y);
            }
            else if (Hor2Ver1)
            {
                NewProjectButton.SetPosition(445, 108);
                OpenProjectButton.SetPosition(690, 108);
                TutorialsButton.SetPosition(445, 329);
            }
            else if (Ver3)
            {
                NewProjectButton.SetPosition(520, 4);
                OpenProjectButton.SetPosition(520, 210);
                TutorialsButton.SetPosition(520, 415);
            }
            else if (Invis)
            {
                NewProjectButton.SetVisible(false);
                OpenProjectButton.SetVisible(false);
                TutorialsButton.SetVisible(false);
            }

            YoutubeButton.SetVisible(true);
            TwitterButton.SetVisible(true);
            if (HorSM)
            {
                YoutubeButton.SetPosition(Size.Width - 89, Size.Height - 87);
                TwitterButton.SetPosition(Size.Width - 182, Size.Height - 87);
            }
            else if (VerSM)
            {
                YoutubeButton.SetPosition(Size.Width - 89, Size.Height - 87);
                TwitterButton.SetPosition(Size.Width - 89, Size.Height - 180);
            }
            else if (InvisSM)
            {
                YoutubeButton.SetVisible(false);
                TwitterButton.SetVisible(false);
            }
            #endregion
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            if (!WidgetIM.Hovering)
            {
                Sprites["filesel"].Visible = false;
                SelectedIndex = -1;
            }
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!WidgetIM.Hovering)
            {
                Sprites["filesel"].Visible = false;
                SelectedIndex = -1;
                return;
            }
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y - Sprites["files"].Y;
            int index = (int) Math.Floor(ry / 48d);
            if (rx < 30 || rx >= 340 || ry <= 0 || index >= RecentCapacity || ry % 48 >= 38 || index >= Editor.GeneralSettings.RecentFiles.Count)
            {
                Sprites["filesel"].Visible = false;
                SelectedIndex = -1;
                return;
            }
            Sprites["filesel"].Visible = true;
            Sprites["filesel"].Y = 2 + Sprites["files"].Y + index * 48;
            SelectedIndex = index;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (Sprites["filesel"].Visible && SelectedIndex > -1)
            {
                LoadRecentProject(SelectedIndex);
            }
        }

        public void NewProject()
        {
            Editor.NewProject();
        }

        public void OpenProject()
        {
            Editor.OpenProject();
        }

        public void LoadRecentProject(int index)
        {
            if (!System.IO.File.Exists(Editor.GeneralSettings.RecentFiles[index][1]))
            {
                MessageBox box = new MessageBox("Error", "No project file could be found in this folder.");
                box.OnDisposing += delegate (object sender, EventArgs e)
                {
                    Editor.GeneralSettings.RecentFiles.RemoveAt(index);
                    SizeChanged(null, new SizeEventArgs(Size));
                    MouseMoving(null, Graphics.LastMouseEvent);
                };
                return;
            }
            Data.SetProjectPath(Editor.GeneralSettings.RecentFiles[index][1]);
            Window.CreateEditor();
            Editor.MakeRecentProject();
        }

        public void ShowTutorials()
        {
            new MessageBox("Oops!", "MK does not have a dedicated wiki yet. You may find the information you're looking for on Discord, Twitter or GitHub, however.");
        }
    }

    public class HomeScreenButton : Widget
    {
        public string Text { get; protected set; } = "";

        public HomeScreenButton(object Parent, string Name = "homeScreenButton")
            : base(Parent, Name)
        {
            SetSize(237, 213);
            Sprites["bg"] = new Sprite(this.Viewport, "home_button.png");
            Sprites["icon"] = new Sprite(this.Viewport);
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].Y = 154;
            Sprites["sel"] = new Sprite(this.Viewport, new Bitmap(224, 200));
            Sprites["sel"].X = Sprites["sel"].Y = 6;
            Sprites["sel"].Bitmap.Unlock();
            Sprites["sel"].Bitmap.DrawRect(0, 0, 224, 200, Color.WHITE);
            Sprites["sel"].Bitmap.DrawRect(1, 1, 222, 198, Color.WHITE);
            Sprites["sel"].Bitmap.Lock();
            Sprites["sel"].Visible = false;
            WidgetIM.OnHoverChanged += HoverChanged;
        }

        public void SetIcon(string path)
        {
            if (Sprites["icon"].Bitmap != null) Sprites["icon"].Bitmap.Dispose();
            Sprites["icon"].Bitmap = new Bitmap(path);
            Sprites["icon"].X = 119 - Sprites["icon"].Bitmap.Width / 2;
            Sprites["icon"].Y = 83 - Sprites["icon"].Bitmap.Height / 2;
        }

        public void SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
                if (string.IsNullOrEmpty(Text)) return;
                Sprites["text"].Bitmap = new Bitmap(212, 47);
                Sprites["text"].Bitmap.Unlock();
                Sprites["text"].Bitmap.Font = Font.Get("Fonts/Ubuntu-B", 20);
                Sprites["text"].Bitmap.DrawText(Text, 118, 12, Color.WHITE, DrawOptions.CenterAlign);
                Sprites["text"].Bitmap.Lock();
            }
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            Sprites["sel"].Visible = WidgetIM.Hovering;
        }
    }
}
