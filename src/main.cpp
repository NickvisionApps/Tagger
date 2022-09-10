#include "ui/application.hpp"

using namespace NickvisionTagger::UI;

/**
 * The main functions
 *
 * @param The number of arguments
 * @param The array of arguments
 *
 * @returns The application exit code
 */
int main(int argc, char* argv[])
{
    Application app("org.nickvision.tagger");
    return app.run(argc, argv);
}
