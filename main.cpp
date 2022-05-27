#include "ui/application.h"

int main(int argc, char* argv[])
{
    NickvisionTagger::UI::Application app{"org.nickvision.tagger"};
    return app.run(argc, argv);
}
