export interface WeatherData {
  city: string;
  country?: string;
  adminRegion?: string;
  latitude: number;
  longitude: number;
  currentTime?: string;
  currentTemperature?: number;
  currentWindSpeed?: number;
  currentRelativeHumidity?: number;
}
