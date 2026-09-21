using MauiAppTempoAgora.Models;
using MauiAppTempoAgora.Services;
using System;
using System.Diagnostics;

namespace MauiAppTempoAgora
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked_Previsao(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txt_cidade.Text))
                {
                    Tempo? t = await DataService.GetPrevisao(txt_cidade.Text);

                    if (t != null)
                    {
                        string dados_previsao = "";

                        // Exibição dos dados em português
                        dados_previsao = $"Latitude: {t.lat} \n" +
                                         $"Longitude: {t.lon} \n" +
                                         $"Nascer do Sol: {t.sunrise} \n" +
                                         $"Pôr do Sol: {t.sunset} \n" +
                                         $"Temperatura Máxima: {t.temp_max} °C\n" +
                                         $"Temperatura Mínima: {t.temp_min} °C\n" +
                                         $"Descrição do Clima: {char.ToUpper(t.description[0]) + t.description.Substring(1)} \n" +
                                         $"Velocidade do Vento: {t.speed} m/s \n" +
                                         $"Visibilidade: {t.visibility / 1000.0} km";

                        lbl_res.Text = dados_previsao;

                        string mapa = $"https://embed.windy.com/embed.html?" +
                                      $"type=map&location=coordinates&metricRain=mm&metricTemp=°C" +
                                      $"&metricWind=km/h&zoom=5&overlay=wind&product=ecmwf&level=surface" +
                                      $"&lat={t.lat.ToString().Replace(",", ".")}&lon={t.lon.ToString().Replace(",", ".")}";

                        wv_mapa.Source = mapa;

                        Debug.WriteLine(mapa);
                    }
                    else
                    {
                        lbl_res.Text = "Sem dados de previsão.";
                    }
                }
                else
                {
                    lbl_res.Text = "Preencha o nome da cidade.";
                }
            }
            catch (Exception ex)
            {
                // Aqui entram as mensagens específicas vindas do DataService:
                // - "Cidade não encontrada. Verifique o nome digitado."
                // - "Sem conexão com a internet. Verifique sua rede."
                // - "Erro ao obter previsão. Tente novamente."
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        private async void Button_Clicked_Localizacao(object sender, EventArgs e)
        {
            try
            {
                GeolocationRequest request = new GeolocationRequest(
                    GeolocationAccuracy.Medium,
                    TimeSpan.FromSeconds(10)
                );

                Location? local = await Geolocation.Default.GetLocationAsync(request);

                if (local != null)
                {
                    string local_disp = $"Latitude: {local.Latitude} \n" +
                                        $"Longitude: {local.Longitude}";

                    lbl_coords.Text = local_disp;

                    // pega nome da cidade que está nas coordenadas.
                    GetCidade(local.Latitude, local.Longitude);
                }
                else
                {
                    lbl_coords.Text = "Nenhuma localização encontrada.";
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Erro: Dispositivo não suporta", fnsEx.Message, "OK");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                await DisplayAlert("Erro: Localização desabilitada", fneEx.Message, "OK");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Erro: Permissão de localização", pEx.Message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        private async void GetCidade(double lat, double lon)
        {
            try
            {
                IEnumerable<Placemark> places = await Geocoding.Default.GetPlacemarksAsync(lat, lon);

                Placemark? place = places.FirstOrDefault();

                if (place != null)
                {
                    txt_cidade.Text = place.Locality;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro: Obtenção do nome da cidade", ex.Message, "OK");
            }
        }
    }
}
