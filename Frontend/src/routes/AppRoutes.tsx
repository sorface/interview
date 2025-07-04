import React, { FunctionComponent, useContext } from 'react';
import { Routes, Route, useLocation, matchPath } from 'react-router-dom';
import { inviteParamName, pathnames } from '../constants';
import { Home } from '../pages/Home/Home';
import { Rooms, RoomsPageMode } from '../pages/Rooms/Rooms';
import { Questions } from '../pages/Questions/Questions';
import { NotFound } from '../pages/NotFound/NotFound';
import { Room } from '../pages/Room/Room';
import { Session } from '../pages/Session/Session';
import { RoomParticipants } from '../pages/RoomParticipants/RoomParticipants';
import { ProtectedRoute } from './ProtectedRoute';
import { User } from '../types/user';
import { Terms } from '../pages/Terms/Terms';
import { CategoriesAdmin } from '../pages/CategoriesAdmin/CategoriesAdmin';
import { checkAdmin } from '../utils/checkAdmin';
import { CategoriesCreate } from '../pages/CategoriesCreate/CategoriesCreate';
import { Gap } from '../components/Gap/Gap';
import { RoomReview } from '../pages/RoomReview/RoomReview';
import { RoomAnaytics } from '../pages/RoomAnaytics/RoomAnaytics';
import { LogoutError } from '../pages/LogoutError/LogoutError';
import { QuestionsArchive } from '../pages/QuestionsArchive/QuestionsArchive';
import { QuestionTrees } from '../pages/QuestionTrees/QuestionTrees';
import { QuestionTreeCreate } from '../pages/QuestionTreeCreate/QuestionTreeCreate';
import { Roadmap } from '../pages/Roadmap/Roadmap';
import { CategoriesArchive } from '../pages/CategoriesArchive/CategoriesArchive';
import { Roadmaps } from '../pages/Roadmaps/Roadmaps';
import { RoadmapCreate } from '../pages/RoadmapCreate/RoadmapCreate';
import { RoadmapsArchive } from '../pages/RoadmapsArchive/RoadmapsArchive';
import { BusinessAnalytic } from '../pages/BusinessAnalytic/BusinessAnalytic';
import { NavMenu } from '../components/NavMenu/NavMenu';
import { DeviceContext } from '../context/DeviceContext';
import { QuestionsRootCategories } from '../pages/QuestionsRootCategories/QuestionsRootCategories';
import { QuestionsSubCategories } from '../pages/QuestionsSubCategories/QuestionsSubCategories';

interface AppRoutesProps {
  user: User | null;
}

export const AppRoutes: FunctionComponent<AppRoutesProps> = ({ user }) => {
  const admin = checkAdmin(user);
  const device = useContext(DeviceContext);
  const location = useLocation();
  const fullScreenPage = matchPath(
    { path: pathnames.room.replace(`/:${inviteParamName}?`, ''), end: false },
    location.pathname,
  );
  const roomsPage = matchPath(
    { path: pathnames.highlightRooms },
    location.pathname,
  );
  const authenticated = !!user;
  const hasNavMenu = !fullScreenPage && authenticated;

  return (
    <>
      {hasNavMenu && <NavMenu admin={admin} />}
      <div
        className={`App ${fullScreenPage ? 'full-screen-page' : ''} ${device === 'Desktop' ? 'px-[2.5rem]' : 'px-[1.25rem]'}`}
      >
        <div
          className={`App-content ${device === 'Desktop' && !fullScreenPage && !roomsPage ? 'px-[13.625rem]' : ''}`}
        >
          <Routes>
            <Route path={pathnames.home} element={<Home />} />
            <Route path={pathnames.terms} element={<Terms />} />
            <Route path={pathnames.logoutError} element={<LogoutError />} />
            <Route
              path={pathnames.roomsParticipants}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoomParticipants />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.roomReview}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoomReview />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.roomAnalytics}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoomAnaytics />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.room}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Room />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.highlightRooms}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Rooms mode={RoomsPageMode.Home} />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.currentRooms}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Rooms mode={RoomsPageMode.Current} />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.closedRooms}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Rooms mode={RoomsPageMode.Closed} />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.questionsArchive}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <QuestionsArchive />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.questionsRootCategories}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <QuestionsRootCategories />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.questionsSubCategories}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <QuestionsSubCategories />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.questions}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Questions />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.session}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Session />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.categoriesCreate}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <CategoriesCreate edit={false} />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.categoriesEdit}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <CategoriesCreate edit={true} />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.categoriesArchive}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <CategoriesArchive />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.categories}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <CategoriesAdmin />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.questionTrees}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <QuestionTrees />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.questionTreeCreate}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <QuestionTreeCreate edit={false} />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.questionTreeEdit}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <QuestionTreeCreate edit={true} />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.roadmapsArchive}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoadmapsArchive />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.roadmapCreate}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoadmapCreate edit={false} />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.roadmapEdit}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <RoadmapCreate edit={true} />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.roadmap}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Roadmap />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.roadmaps}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <Roadmaps />
                </ProtectedRoute>
              }
            />
            <Route
              path={pathnames.businessAnalytic}
              element={
                <ProtectedRoute allowed={authenticated}>
                  <BusinessAnalytic />
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<NotFound />} />
          </Routes>
          {!fullScreenPage && <Gap sizeRem={0.5} />}
        </div>
      </div>
    </>
  );
};
