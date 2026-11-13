
export interface PlatformPrice {
  platform: string;
  currentPrice: number | null;
  historicalLow: number | null;
  averagePrice: number | null;
  currency: string | null;
  url: string | null;
}

export interface GamePriceComparison {
  gameId: number;
  steamAppId: number;
  platforms: PlatformPrice[];
}
