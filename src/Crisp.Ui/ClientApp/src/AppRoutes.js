import Home from "./components/Home";
import ThreatModels from "./components/ThreatModels";
import AddThreatModel from "./components/AddThreatModel";
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
        element: <AddThreatModel />
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
