#include <gtk/gtk.h>
#include <webkitgtk-4.0/webkit2/webkit2.h>


int main(int argc, char* argv[]) {

    gtk_init(&argc, &argv);

    GtkWidget *window = gtk_window_new(GTK_WINDOW_TOPLEVEL);
    gtk_window_set_default_size(GTK_WINDOW(window), 800, 600);
    g_signal_connect(window, "destroy", G_CALLBACK(gtk_main_quit), NULL);

    WebKitWebView *web_view = WEBKIT_WEB_VIEW(webkit_web_view_new());

    WebKitSettings *settings = webkit_web_view_get_settings(web_view);
    //WebKitSettings *settings = webkit_web_view_get_settings(WEBKIT_WEB_VIEW(web_view));
    g_object_set(G_OBJECT(settings), "enable-frame-flattening", FALSE, NULL);
    g_object_set(G_OBJECT(settings), "enable-page-cache", FALSE, NULL);
    g_object_set(G_OBJECT(settings), "enable-accelerated-2d-canvas", TRUE, NULL);

    //g_object_set(G_OBJECT(settings), "enable-touch-events", TRUE, NULL);
    //g_object_set(G_OBJECT(settings), "enable-full-content-zoom", FALSE, NULL);
    webkit_settings_set_enable_page_cache(settings, FALSE);
    //webkit_web_view_set_settings(WEBKIT_WEB_VIEW(web_view), settings);
    //webkit_settings_set_enable_touch_events(settings, TRUE);

    //WebKitSettings *settings = webkit_web_view_get_settings(WEBKIT_WEB_VIEW(webview));
    


    webkit_web_view_set_settings(web_view, settings);


    gtk_container_add(GTK_CONTAINER(window), GTK_WIDGET(web_view));

    webkit_web_view_load_uri(web_view, "http://127.0.0.1/");
    //webkit_web_view_load_uri(web_view, "http://192.168.0.100:5015/");

    GtkCssProvider *provider = gtk_css_provider_new();
    gtk_css_provider_load_from_data(provider, "* { background-color: #000; }", -1, NULL);
    GdkScreen *screen = gdk_screen_get_default();
    gtk_style_context_add_provider_for_screen(screen, GTK_STYLE_PROVIDER(provider), GTK_STYLE_PROVIDER_PRIORITY_APPLICATION);
    g_object_unref(provider);

    gtk_window_fullscreen(GTK_WINDOW(window));

    gtk_widget_show_all(window);

    gtk_main();

    return 0;
    //https://idroot.us/install-postgresql-debian-12/




}

