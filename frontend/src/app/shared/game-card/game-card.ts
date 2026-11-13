import { Component, input, output } from '@angular/core';
import { GameModel } from '../../core/models/game.model';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';


@Component({
  imports: [RouterLink, DecimalPipe],
  selector: 'app-game-card',
  styleUrl: './game-card.scss',
  templateUrl: './game-card.html',
})
export class GameCard {
  game = input.required<GameModel>();
  isFavorite = input(false);
  favoriteBusy = input(false);
  favoriteToggled = output<number>();
  toggleFavorite(): void {
    this.favoriteToggled.emit(this.game().id);
  }
}
