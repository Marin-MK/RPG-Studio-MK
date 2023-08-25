using RPGStudioMK.Game;
using RPGStudioMK.Utility;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

partial class DataTypeSpecies
{
	void DrawMoveLabels(DataContainer container)
	{
		container.Sprites["txt"] = new Sprite(container.Viewport);
		container.Sprites["txt"].X = 50;
		container.Sprites["txt"].Y = 46;
		container.Sprites["txt"].Bitmap = new Bitmap(900, 26);
		container.Sprites["txt"].Bitmap.Unlock();
		container.Sprites["txt"].Bitmap.Font = Fonts.Paragraph;
		container.Sprites["txt"].Bitmap.DrawText("Level", 16, 4, Color.WHITE);
		container.Sprites["txt"].Bitmap.DrawText("Move", 100, 4, Color.WHITE);
		container.Sprites["txt"].Bitmap.DrawText("Type", 250, 4, Color.WHITE);
		container.Sprites["txt"].Bitmap.DrawText("Category", 400, 4, Color.WHITE);
		container.Sprites["txt"].Bitmap.DrawText("Accuracy", 550, 4, Color.WHITE);
		container.Sprites["txt"].Bitmap.DrawText("Power", 690, 4, Color.WHITE);
		container.Sprites["txt"].Bitmap.DrawText("Priority", 800, 4, Color.WHITE);
		container.Sprites["txt"].Bitmap.Lock();
	}

	void CreateLevelContainer(DataContainer parent, Species spc)
	{
		DrawMoveLabels(parent);

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
		MoveDropdownBox moveBox = new MoveDropdownBox(addContainer);
		moveBox.SetPosition(110, 3);
		moveBox.SetWidth(200);
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
			(int lvl, MoveResolver mov) newItem = (levelBox.Value, moveBox.Move);
			int idx = spc.Moves.FindIndex(it => it.Level >= newItem.lvl);
			if (idx == -1) idx = spc.Moves.Count;
			spc.Moves.Insert(idx, newItem);
			MoveEntryWidget mew = new MoveEntryWidget(panel, idx);
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
		DrawMoveLabels(parent);

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
		MoveDropdownBox moveBox = new MoveDropdownBox(addContainer);
		moveBox.SetPosition(110, 3);
		moveBox.SetWidth(200);
		moveBox.SetSelectedIndex(0);

		addButton.OnClicked += _ =>
		{
			(int lvl, MoveResolver mov) newItem = (0, moveBox.Move);
			spc.Moves.Insert(0, newItem);
			MoveEntryWidget mew = new MoveEntryWidget(panel, 0);
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

	void CreateTMContainer(DataContainer parent, Species spc)
	{
		List<Item> hms = Data.Sources.HMs;
		List<Item> tms = Data.Sources.TMs;
		tms.RemoveAll(item => hms.Any(hm => hm.Move.Move == item.Move.Move));

		ColumnFormatter posHelper = new ColumnFormatter(tms.Count, 4);
		posHelper.SetX(53);
		posHelper.SetY(60);
		posHelper.SetXDistance(220);
		posHelper.SetYDistance(30);
		
		for (int i = 0; i < tms.Count; i++)
		{
			Item item = tms[i];
			CheckBox box = new CheckBox(parent);
			box.SetPosition(posHelper.GetPosition(i));
			box.SetText($"{item.Name} - {item.Move.Move.Name}");
			box.SetChecked(spc.TutorMoves.Any(m => m.Move == item.Move.Move));
			box.OnCheckChanged += _ =>
			{
				if (box.Checked) spc.TutorMoves.Add(item.Move);
				else spc.TutorMoves.RemoveAll(m => m.Move == item.Move.Move);
			};
		}

		for (int i = 0; i < hms.Count; i++)
		{
			Item item = hms[i];
			CheckBox box = new CheckBox(parent);
			box.SetPosition(posHelper.X, posHelper.GetMaxY() + posHelper.YDistance * i + 30);
			box.SetText($"{item.Name} - {item.Move.Move.Name}");
			box.SetChecked(spc.TutorMoves.Any(m => m.Move == item.Move.Move));
			box.OnCheckChanged += _ =>
			{
				if (box.Checked) spc.TutorMoves.Add(item.Move);
				else spc.TutorMoves.RemoveAll(m => m.Move == item.Move.Move);
			};
		}

		parent.UpdateSize();
	}

	void CreateEggMovesContainer(DataContainer parent, Species spc)
	{
		DrawMoveLabels(parent);

		VStackPanel panel = new VStackPanel(parent);
		panel.SetPosition(50, 80);
		panel.SetWidth(900);
		foreach (MoveResolver Move in spc.EggMoves)
		{
			MoveEntryWidget mew = new MoveEntryWidget(panel);
			mew.SetMove(0, Move);
			mew.SetMargins(0, 3);
			mew.OnButtonClicked += _ =>
			{
				spc.EggMoves.Remove(Move);
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
		moveBox.SetItems(Data.Sources.Moves);
		moveBox.SetSelectedIndex(0);

		addButton.OnClicked += _ =>
		{
			MoveResolver newMove = (MoveResolver) (Move) moveBox.SelectedItem?.Object;
			spc.EggMoves.Insert(0, newMove);
			MoveEntryWidget mew = new MoveEntryWidget(panel, 0);
			mew.SetMove(0, newMove);
			mew.SetMargins(0, 3);
			mew.OnButtonClicked += _ =>
			{
				spc.EggMoves.Remove(newMove);
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

	void CreateTutorMovesContainer(DataContainer parent, Species spc)
	{
		DrawMoveLabels(parent);

		VStackPanel panel = new VStackPanel(parent);
		panel.SetPosition(50, 80);
		panel.SetWidth(900);
		foreach (MoveResolver Move in spc.TutorMoves)
		{
			// Do not list TMs/HMs in the tutor moves section, as they have their own section.
			if (Data.Sources.TMs.Any(tm => tm.Move.Move == Move.Move) ||
				Data.Sources.HMs.Any(hm => hm.Move.Move == Move.Move)) continue;
			MoveEntryWidget mew = new MoveEntryWidget(panel);
			mew.SetMove(0, Move);
			mew.SetMargins(0, 3);
			mew.OnButtonClicked += _ =>
			{
				spc.TutorMoves.Remove(Move);
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
		MoveDropdownBox moveBox = new MoveDropdownBox(addContainer);
		moveBox.SetPosition(110, 3);
		moveBox.SetWidth(200);
		// Only list moves that are not a TM/HM, as those moves are already listed in the TM/HM section.
		moveBox.SetItems(Data.Sources.Moves.FindAll(move => !Data.Sources.TMs.Any(tm => tm.Move.Move == (Move) move.Object) && !Data.Sources.HMs.Any(hm => hm.Move.Move == (Move) move.Object)));
		moveBox.SetSelectedIndex(0);

		addButton.OnClicked += _ =>
		{
			MoveResolver newMove = moveBox.Move;
			spc.TutorMoves.Insert(0, newMove);
			MoveEntryWidget mew = new MoveEntryWidget(panel, 0);
			mew.SetMove(0, newMove);
			mew.SetMargins(0, 3);
			mew.OnButtonClicked += _ =>
			{
				spc.TutorMoves.Remove(newMove);
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
