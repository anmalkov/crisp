import Home from "./components/Home";
import ThreatModels from "./components/ThreatModels";
import ThreatModel from "./components/threat-model/ThreatModel";
import ThreatModelReport from "./components/ThreatModelReport";
import Recommendations from "./components/Recommendations";
import Resources from "./components/Resources";
import ThreatsMapping from "./components/ThreatMapping";

const AppRoutes = [
    {
        index: true,
        element: <Home />
    },
    {
        path: '/threatmodels',
        element: <ThreatModels />
    },
    {
        path: '/addthreatmodel',
        element: <ThreatModel />
    },
    {
        path: '/threatmodelreport',
        element: <ThreatModelReport />
    },
    {
        path: '/recommendations',
        element: <Recommendations />
    },
    {
        path: '/resources',
        element: <Resources />
    },
    {
        path: '/map',
        element: <ThreatsMapping />
    }
];

export default AppRoutes;
