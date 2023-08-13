using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

partial class DataTypeSpecies
{
	void CreateLevelContainer(DataContainer parent, Species spc)
	{
		parent.Sprites["txt"] = new Sprite(parent.Viewport);
		parent.Sprites["txt"].X = 50;
		parent.Sprites["txt"].Y = 46;
		parent.Sprites["txt"].Bitmap = new Bitmap(900, 26);
		parent.Sprites["txt"].Bitmap.Unlock();
		parent.Sprites["txt"].Bitmap.Font = Fonts.Paragraph;
		parent.Sprites["txt"].Bitmap.DrawText("Level", 16, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Move", 100, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Type", 250, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Category", 400, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Accuracy", 550, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("PP", 700, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Priority", 800, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.Lock();

		VStackPanel panel = new VStackPanel(parent);
		panel.SetPosition(50, 80);
		panel.SetWidth(900);
		foreach ((int Level, MoveResolver Move) item in spc.Moves)
		{
			if (item.Level == 0) continue;
			MoveEntryWidget mew = new MoveEntryWidget(panel);
			mew.SetMove(item.Level, item.Move);
			mew.SetMargins(0, 3);
			mew.OnButtonClicked += _ =>
			{
				spc.Moves.Remove(item);
				mew.Dispose();
				panel.UpdateLayout();
				parent.UpdateSize();
			};
		}

		Container addContainer = new Container(panel);
		addContainer.SetMargins(0, 10);

		Button addButton = new Button(addContainer);
		addButton.SetText("Add");
		addButton.SetWidth(100);
		DropdownBox moveBox = new DropdownBox(addContainer);
		moveBox.SetPosition(110, 3);
		moveBox.SetWidth(200);
		moveBox.SetItems(Data.Sources.MovesListItemsAlphabetical);
		moveBox.SetSelectedIndex(0);

		Label setLabel = new Label(addContainer);
		setLabel.SetPosition(330, 6);
		setLabel.SetText("at level");

		NumericBox levelBox = new NumericBox(addContainer);
		levelBox.SetPosition(390, 3);
		levelBox.SetWidth(100);
		levelBox.SetMinValue(1);
		levelBox.SetMaxValue(100);

		addButton.OnClicked += _ =>
		{
			(int lvl, MoveResolver mov) newItem = (levelBox.Value, (MoveResolver) (Move) moveBox.SelectedItem?.Object);
			int idx = spc.Moves.FindIndex(it => it.Level >= newItem.lvl);
			if (idx == -1) idx = spc.Moves.Count;
			MoveEntryWidget mew = null;
			spc.Moves.Insert(idx, newItem);
			mew = new MoveEntryWidget(panel, idx);
			mew.SetMove(newItem.lvl, newItem.mov);
			mew.SetMargins(0, 3);
			mew.OnButtonClicked += _ =>
			{
				spc.Moves.Remove(newItem);
				mew.Dispose();
				panel.UpdateLayout();
				parent.UpdateSize();
			};
			panel.UpdateLayout();
			parent.UpdateSize();
		};

		panel.UpdateLayout();
		parent.UpdateSize();
	}

	void CreateEvoMovesContainer(DataContainer parent, Species spc)
	{
		parent.Sprites["txt"] = new Sprite(parent.Viewport);
		parent.Sprites["txt"].X = 50;
		parent.Sprites["txt"].Y = 46;
		parent.Sprites["txt"].Bitmap = new Bitmap(900, 26);
		parent.Sprites["txt"].Bitmap.Unlock();
		parent.Sprites["txt"].Bitmap.Font = Fonts.Paragraph;
		parent.Sprites["txt"].Bitmap.DrawText("Level", 16, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Move", 100, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Type", 250, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Category", 400, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Accuracy", 550, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("PP", 700, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.DrawText("Priority", 800, 4, Color.WHITE);
		parent.Sprites["txt"].Bitmap.Lock();

		VStackPanel panel = new VStackPanel(parent);
		panel.SetPosition(50, 80);
		panel.SetWidth(900);
		foreach ((int Level, MoveResolver Move) item in spc.Moves)
		{
			if (item.Level != 0) continue;
			MoveEntryWidget mew = new MoveEntryWidget(panel);
			mew.SetMove(item.Level, item.Move);
			mew.SetMargins(0, 3);
			mew.OnButtonClicked += _ =>
			{
				spc.Moves.Remove(item);
				mew.Dispose();
				panel.UpdateLayout();
				parent.UpdateSize();
			};
		}

		Container addContainer = new Container(panel);
		addContainer.SetMargins(0, 10);

		Button addButton = new Button(addContainer);
		addButton.SetText("Add");
		addButton.SetWidth(100);
		DropdownBox moveBox = new DropdownBox(addContainer);
		moveBox.SetPosition(110, 3);
		moveBox.SetWidth(200);
		moveBox.SetItems(Data.Sources.MovesListItemsAlphabetical);
		moveBox.SetSelectedIndex(0);

		addButton.OnClicked += _ =>
		{
			(int lvl, MoveResolver mov) newItem = (0, (MoveResolver) (Move) moveBox.SelectedItem?.Object);
			MoveEntryWidget mew = new MoveEntryWidget(panel, 0);
			spc.Moves.Insert(0, newItem);
			mew.SetMove(newItem.lvl, newItem.mov);
			mew.SetMargins(0, 3);
			mew.OnButtonClicked += _ =>
			{
				spc.Moves.Remove(newItem);
				mew.Dispose();
				panel.UpdateLayout();
				parent.UpdateSize();
			};
			panel.UpdateLayout();
			parent.UpdateSize();
		};

		panel.UpdateLayout();
		parent.UpdateSize();
	}
}
